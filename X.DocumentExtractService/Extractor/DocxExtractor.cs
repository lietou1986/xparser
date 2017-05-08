using Dorado.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Extractor
{
    internal class DocxExtractor : DocumentExtractor
    {
        private const string ContentTypeNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

        private const string WordprocessingMlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        private const string DocumentXmlXPath = "/t:Types/t:Override[@ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"]";

        private const string BodyXPath = "/w:document/w:body";

        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            if (!extensionName.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("只支持.docx文档");
            }
            return true;
        }

        protected override Picture[] ExtractImages(string extensionName, byte[] data)
        {
            Size imageSize;
            List<Picture> pictures = new List<Picture>();
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (ZipFile zipFiles = new ZipFile(memoryStream))
                {
                    foreach (ZipEntry zipFile in zipFiles)
                    {
                        if (zipFile.Name.IndexOf("word/media") != 0)
                        {
                            continue;
                        }
                        Picture picture = new Picture();
                        using (MemoryStream memoryStream1 = new MemoryStream())
                        {
                            using (Stream inputStream = zipFiles.GetInputStream(zipFile))
                            {
                                inputStream.CopyTo(memoryStream1);
                            }
                            picture.Extension = Path.GetExtension(zipFile.Name).Replace(".", "");
                            memoryStream1.Position = 0;
                            imageSize = GetImageSize(memoryStream1);
                            memoryStream1.Position = 0;
                            picture.Data = memoryStream1.ReadAllBytes();
                        }
                        picture.Width = imageSize.Width;
                        picture.Height = imageSize.Height;
                        pictures.Add(picture);
                    }
                }
            }
            return pictures.ToArray();
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            string str = FindDocumentXmlLocation(data);
            if (string.IsNullOrEmpty(str))
            {
                throw new NotSupportedException("不是一个有效的docx文档");
            }
            StringBuilder stringBuilder = new StringBuilder();
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (ZipFile zipFiles = new ZipFile(memoryStream))
                {
                    foreach (ZipEntry zipFile in zipFiles)
                    {
                        if (string.Compare(zipFile.Name, str, true) != 0)
                        {
                            continue;
                        }
                        XmlDocument xmlDocument = new XmlDocument()
                        {
                            PreserveWhitespace = true
                        };
                        using (Stream inputStream = zipFiles.GetInputStream(zipFile))
                        {
                            xmlDocument.Load(inputStream);
                        }
                        XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
                        xmlNamespaceManagers.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                        XmlNode xmlNodes = xmlDocument.DocumentElement.SelectSingleNode("/w:document/w:body", xmlNamespaceManagers);
                        if (xmlNodes != null)
                        {
                            stringBuilder.Append(ReadNode(xmlNodes));
                            return stringBuilder.ToString();
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private static string FindDocumentXmlLocation(byte[] data)
        {
            string str;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (ZipFile zipFiles = new ZipFile(memoryStream))
                {
                    IEnumerator enumerator = zipFiles.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            ZipEntry current = (ZipEntry)enumerator.Current;
                            if (string.Compare(current.Name, "[Content_Types].xml", true) != 0)
                            {
                                continue;
                            }
                            XmlDocument xmlDocument = new XmlDocument()
                            {
                                PreserveWhitespace = true
                            };
                            using (Stream inputStream = zipFiles.GetInputStream(current))
                            {
                                xmlDocument.Load(inputStream);
                            }
                            XmlNamespaceManager xmlNamespaceManagers = new XmlNamespaceManager(xmlDocument.NameTable);
                            xmlNamespaceManagers.AddNamespace("t", "http://schemas.openxmlformats.org/package/2006/content-types");
                            XmlNode xmlNodes = xmlDocument.DocumentElement.SelectSingleNode("/t:Types/t:Override[@ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"]", xmlNamespaceManagers);
                            if (xmlNodes == null)
                            {
                                break;
                            }
                            str = ((XmlElement)xmlNodes).GetAttribute("PartName").TrimStart('/');
                            return str;
                        }
                        return null;
                    }
                    finally
                    {
                        IDisposable disposable = enumerator as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        private static string ReadNode(XmlNode node)
        {
            if (node == null || node.NodeType != XmlNodeType.Element)
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                string localName = childNode.LocalName;
                switch (localName)
                {
                    case "t":
                        {
                            stringBuilder.Append(childNode.InnerText.TrimEnd());
                            string attribute = ((XmlElement)childNode).GetAttribute("xml:space");
                            if (string.IsNullOrEmpty(attribute) || !(attribute == "preserve"))
                            {
                                continue;
                            }
                            stringBuilder.Append(' ');
                            break;
                        }
                    case "cr":
                    case "br":
                        {
                            stringBuilder.Append('\n');
                            break;
                        }
                    case "tab":
                        {
                            stringBuilder.Append("\t");
                            break;
                        }
                    case "p":
                        {
                            stringBuilder.Append(ReadNode(childNode));
                            stringBuilder.Append("\r");
                            break;
                        }
                    case "tc":
                        {
                            string str = ReadNode(childNode);
                            string[] strArrays = str.Split("\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (strArrays.Length <= 1)
                            {
                                stringBuilder.Append(str);
                            }
                            else
                            {
                                stringBuilder.Append(string.Join("\n", strArrays));
                            }
                            break;
                        }
                    case "tr":
                        {
                            string str1 = ReadNode(childNode);
                            stringBuilder.Append(str1.Replace("\r", "\r\a"));
                            stringBuilder.Append("\r\n");
                            break;
                        }
                    default:
                        {
                            stringBuilder.Append(ReadNode(childNode));
                            break;
                        }
                }
            }
            return stringBuilder.ToString();
        }
    }
}