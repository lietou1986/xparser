using Dorado.Core;
using Dorado.Extensions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Extractor
{
    internal class HtmlExtractor : DocumentExtractor
    {
        private static readonly Regex _BlanksRegex;

        private static readonly HashSet<string> _TabTags;

        private static readonly HashSet<string> _NewLineTags;

        private static readonly HashSet<string> _PossibleNewLineTags;

        private static readonly HashSet<string> _IgnoreTags;

        private static readonly Regex _NoneStyle;

        private string m_Text;

        static HtmlExtractor()
        {
            _BlanksRegex = new Regex("\\s+", RegexOptions.Compiled);
            HashSet<string> strs = new HashSet<string> { "td", "dd" };
            _TabTags = strs;
            HashSet<string> strs1 = new HashSet<string> { "br", "hr" };
            _NewLineTags = strs1;
            HashSet<string> strs2 = new HashSet<string>
            {
                "table",
                "tr",
                "div",
                "h1",
                "h2",
                "h3",
                "h4",
                "h5",
                "h6",
                "li",
                "dl",
                "dt",
                "p"
            };
            _PossibleNewLineTags = strs2;
            HashSet<string> strs3 = new HashSet<string> { "style", "script" };
            _IgnoreTags = strs3;
            _NoneStyle = new Regex("display:\\s*none", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private static bool AddNewLine(HtmlNode node)
        {
            if (_NewLineTags.Contains(node.Name))
            {
                return true;
            }
            if (!_PossibleNewLineTags.Contains(node.Name))
            {
                return false;
            }
            if (node.ParentNode != null)
            {
                if ((
                    from n in node.ParentNode.ChildNodes
                    where !n.InnerText.IsNullOrWhiteSpace()
                    select n).Count() == 1 && !_PossibleNewLineTags.Contains(node.ParentNode.Name))
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            return true;
        }

        private static string ChangeUTF8Space(string targetStr)
        {
            string str;
            if (targetStr.IsNullOrWhiteSpace())
            {
                return targetStr;
            }
            try
            {
                byte[] numArray = new byte[] { 194, 160 };
                string str1 = Encoding.UTF8.GetString(numArray);
                str = targetStr.Replace(str1, " ");
            }
            catch (Exception exception)
            {
                LoggerWrapper.Logger.Warn("替换非标准空格时出错", exception.ToString());
                LoggerWrapper.Logger.Warn("替换非标准空格时出错", exception);
                str = targetStr;
            }
            return str;
        }

        private Stream DownloadImage(string src)
        {
            Stream responseStream;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(src);
            try
            {
                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                responseStream = response.GetResponseStream();
            }
            catch (Exception exception)
            {
                LoggerWrapper.Logger.Warn("下载图片失败", "URL={0},{1}", src, exception);
                return null;
            }
            return responseStream;
        }

        protected override Picture[] ExtractImages(string extensionName, byte[] data)
        {
            List<Picture> pictures = new List<Picture>();
            HtmlDocument htmlDocument = new HtmlDocument();
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                htmlDocument.Load(memoryStream);
            }
            HtmlImage[] htmlImage = GetHtmlImage(htmlDocument);
            foreach (HtmlImage htmlImage1 in htmlImage)
            {
                if (!htmlImage1.Src.IsNullOrWhiteSpace())
                {
                    using (Stream stream = DownloadImage(htmlImage1.Src))
                    {
                        if (stream != null)
                        {
                            Picture picture = new Picture()
                            {
                                Extension = Path.GetExtension(htmlImage1.Src)
                            };
                            if (htmlImage1.Width < 0 || htmlImage1.Height < 0)
                            {
                                using (MemoryStream memoryStream1 = new MemoryStream())
                                {
                                    stream.CopyTo(memoryStream1);
                                    Size imageSize = GetImageSize(memoryStream1);
                                    picture.Width = imageSize.Width;
                                    picture.Height = imageSize.Height;
                                    memoryStream1.Position = 0;
                                    picture.Data = memoryStream1.ReadAllBytes();
                                }
                            }
                            else
                            {
                                picture.Data = stream.ReadAllBytes();
                                picture.Width = (int)htmlImage1.Width;
                                picture.Height = (int)htmlImage1.Height;
                            }
                            pictures.Add(picture);
                        }
                    }
                }
            }
            return pictures.ToArray();
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            string text = (new TextExtractor()).Extract(extensionName, data, ExtractOption.Text).Text;
            m_Text = text;
            return HtmlToText(text);
        }

        private static double GetDoubleValue(string text)
        {
            double num;
            if (double.TryParse(text, out num))
            {
                return num;
            }
            return -1;
        }

        private HtmlImage[] GetHtmlImage(HtmlDocument htmlDoc)
        {
            List<HtmlImage> htmlImages = new List<HtmlImage>();
            foreach (HtmlNode htmlNode in
                from node in htmlDoc.DocumentNode.Descendants()
                where node.Name.Equals("img", StringComparison.InvariantCultureIgnoreCase)
                select node)
            {
                if (htmlNode.Attributes["src"] == null || htmlNode.Attributes["src"].Value.IsNullOrWhiteSpace())
                {
                    continue;
                }
                HtmlImage htmlImage = new HtmlImage();
                htmlImage.AddProperty("src", htmlNode.Attributes["src"].Value);
                if (htmlNode.Attributes["width"] != null)
                {
                    htmlImage.Width = GetDoubleValue(htmlNode.Attributes["width"].Value);
                }
                if (htmlNode.Attributes["height"] != null)
                {
                    htmlImage.Height = GetDoubleValue(htmlNode.Attributes["height"].Value);
                }
                htmlImages.Add(htmlImage);
            }
            return htmlImages.ToArray();
        }

        private static string GetTextInHtml(HtmlDocument htmlDoc)
        {
            StringBuilder stringBuilder = new StringBuilder();
            GetTextInHtmlNode(htmlDoc.DocumentNode, stringBuilder);
            if (stringBuilder.Length == 0)
            {
                return string.Empty;
            }
            return ChangeUTF8Space(HttpUtility.HtmlDecode(stringBuilder.ToString()));
        }

        private static void GetTextInHtmlNode(HtmlNode node, StringBuilder sb)
        {
            if (_IgnoreTags.Contains(node.Name))
            {
                return;
            }
            if (AddNewLine(node))
            {
                sb.AppendLine();
            }
            node.ChildNodes.Where((HtmlNode n) =>
            {
                if (_IgnoreTags.Contains(n.Name))
                {
                    return false;
                }
                return !n.Attributes.Any((HtmlAttribute a) =>
                {
                    if (a.Name != "style")
                    {
                        return false;
                    }
                    return _NoneStyle.IsMatch(a.Value);
                });
            }).ToList().ForEach((HtmlNode n) => GetTextInHtmlNode(n, sb));
            if (node.NodeType == HtmlNodeType.Text)
            {
                sb.Append(_BlanksRegex.Replace(node.InnerText, " "));
                return;
            }
            if (_TabTags.Contains(node.Name))
            {
                sb.Append("  ");
                return;
            }
            if (AddNewLine(node))
            {
                sb.AppendLine();
                return;
            }
            if (node.Name.Equals("img", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append("  ");
            }
        }

        internal static string HtmlToText(string html)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return GetTextInHtml(htmlDocument);
        }
    }
}