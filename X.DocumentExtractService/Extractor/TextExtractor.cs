using HtmlAgilityPack;
using System;
using System.IO;
using System.Text;
using Ude;

namespace X.DocumentExtractService.Extractor
{
    internal class TextExtractor : DocumentExtractor
    {
        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            return true;
        }

        private static Encoding DetectEncoding(byte[] data)
        {
            Encoding encoding;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                CharsetDetector charsetDetector = new CharsetDetector();
                charsetDetector.Feed(memoryStream);
                charsetDetector.DataEnd();
                if (charsetDetector.Charset == null)
                {
                    encoding = null;
                }
                else
                {
                    encoding = (!charsetDetector.Charset.Equals("Big-5", StringComparison.OrdinalIgnoreCase) ? Encoding.GetEncoding(charsetDetector.Charset) : Encoding.GetEncoding("Big5"));
                }
            }
            return encoding;
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            string str;
            Encoding encoding = DetectEncoding(data);
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                if (encoding != null)
                {
                    str = encoding.GetString(memoryStream.ToArray());
                }
                else
                {
                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.DetectEncoding(memoryStream);
                    str = htmlDocument.DocumentNode.OuterHtml;
                }
            }
            return str;
        }
    }
}