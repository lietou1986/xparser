using Aspose.Words;
using Aspose.Words.Drawing;
using Dorado.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Extractor
{
    internal class AsposeWordExtractor : DocumentExtractor
    {
        private static readonly Regex RegexHyperlinkEmail;

        private static readonly Regex RegexContentTypeMultipartRelated;

        private static readonly Regex RegexContentTypeTextHtml;

        private static readonly Regex RegexCharset;

        private FileTypeCode m_FileTypeCode = FileTypeCode.Unknown;

        static AsposeWordExtractor()
        {
            RegexHyperlinkEmail = new Regex("\u0013\\s+HYPERLINK \"mailto:([a-zA-Z0-9_%+#&'*/=^`{|}~-](?:\\.?[a-zA-Z0-9_%+#&'*/=^`{|}~-])*@(?:[a-zA-Z0-9_](?:(?:\\.?|-*)[a-zA-Z0-9_])*\\.[a-zA-Z]{2,9}|\\[(?:2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?:2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?:2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?:2[0-4]\\d|25[0-5]|[01]?\\d\\d?)]))\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            RegexContentTypeMultipartRelated = new Regex("Content-Type:\\s*?multipart/related;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            RegexContentTypeTextHtml = new Regex("Content-Type:\\s*?text/html", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            RegexCharset = new Regex("charset=.{2,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            m_FileTypeCode = GetFileTypeCode(data);
            if (m_FileTypeCode == FileTypeCode.Unknown)
            {
                string str = Encoding.UTF8.GetString(data);
                if (RegexContentTypeMultipartRelated.IsMatch(str) && RegexContentTypeTextHtml.IsMatch(str) && !RegexCharset.IsMatch(str))
                {
                    return false;
                }
            }
            return true;
        }

        private Document CreateDocumentByExtensionName(string extensionName, Stream stream)
        {
            Document document = null;
            if (extensionName == null)
            {
                extensionName = string.Empty;
            }
            extensionName = extensionName.ToLower();
            if (extensionName == ".mht")
            {
                document = new Document(stream, new LoadOptions(LoadFormat.Mhtml, null, null));
            }
            else if (extensionName == ".doc")
            {
                document = CreateDocumentWithDoc(stream);
            }
            else
            {
                document = (extensionName == ".rtf" ? new Document(stream, new LoadOptions(LoadFormat.Rtf, null, null)) : new Document(stream));
            }
            return document;
        }

        private Document CreateDocumentWithDoc(Stream stream)
        {
            Document document = null;
            if (m_FileTypeCode != FileTypeCode.Unknown)
            {
                document = new Document(stream, new LoadOptions(LoadFormat.Auto, null, null));
            }
            else
            {
                MemoryStream memoryStream = null;
                try
                {
                    try
                    {
                        document = new Document(stream);
                        memoryStream = new MemoryStream();
                        document.Save(memoryStream, SaveFormat.Doc);
                        memoryStream.Position = 0;
                        document = new Document(memoryStream, new LoadOptions(LoadFormat.Auto, null, null));
                    }
                    catch (Exception exception)
                    {
                        LoggerWrapper.Logger.Warn("AsposeSave", exception.ToString());
                    }
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Close();
                    }
                }
            }
            return document;
        }

        protected override Picture[] ExtractImages(string extensionName, byte[] data)
        {
            List<Picture> pictures = new List<Picture>();
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                foreach (Node childNode in CreateDocumentByExtensionName(extensionName, memoryStream).GetChildNodes(NodeType.Any, true))
                {
                    ImageData imageData = null;
                    if (childNode is Shape)
                    {
                        Shape shape = childNode as Shape;
                        if (shape.HasImage)
                        {
                            imageData = shape.ImageData;
                        }
                    }
                    if (imageData == null)
                    {
                        continue;
                    }
                    pictures.Add(new Picture()
                    {
                        Data = imageData.ImageBytes,
                        Extension = imageData.ImageType.ToString(),
                        Width = imageData.ImageSize.WidthPixels,
                        Height = imageData.ImageSize.HeightPixels
                    });
                }
            }
            return pictures.ToArray();
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            string str;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                StringBuilder stringBuilder = new StringBuilder(CreateDocumentByExtensionName(extensionName, memoryStream).GetText());
                if (stringBuilder.Length > 0)
                {
                    string str1 = stringBuilder.Replace("\a\a", Environment.NewLine).Replace("\a", "  ").ToString();
                    stringBuilder = new StringBuilder(RegexHyperlinkEmail.Replace(str1, (Match match) => match.Groups[1].Value));
                }
                str = stringBuilder.ToString();
                str = Regex.Replace(str, "Evaluation Only. Created with Aspose[\\S|\\s]* Aspose Pty Ltd.", "");
            }
            return str;
        }

        private static FileTypeCode GetFileTypeCode(byte[] data)
        {
            if (data != null && data.Length >= 2)
            {
                int num = 0;
                if (int.TryParse(string.Concat(data[0].ToString(), data[1].ToString()), out num) && Enum.IsDefined(typeof(FileTypeCode), num))
                {
                    return (FileTypeCode)num;
                }
            }
            return FileTypeCode.Unknown;
        }

        private enum FileTypeCode
        {
            Unknown = -1,
            PDF = 3780,
            PSD = 5666,
            HTML = 6033,
            XML = 6063,
            HLP = 6395,
            BMP = 6677,
            GIF = 7173,
            CHM = 7384,
            COM = 7790,
            DLL = 7790,
            EXE = 7790,
            ZIP = 8075,
            REG = 8269,
            RAR = 8297,
            BTSEED = 10056,
            PNG = 13780,
            BAT = 64101,
            LOG = 70105,
            CS = 117115,
            JS = 119105,
            DOC = 208207,
            DOCX = 208207,
            XLS = 208207,
            XLSX = 208207,
            TXT = 210187,
            ASPX = 239187,
            JPG = 255216,
            RDP = 255254,
            SQL = 255254
        }
    }
}