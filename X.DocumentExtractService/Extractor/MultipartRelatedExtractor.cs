using Dorado.Core;
using Dorado.Extensions;
using System;
using System.Text;
using System.Text.RegularExpressions;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Extractor
{
    internal class MultipartRelatedExtractor : DocumentExtractor
    {
        private static readonly Regex RegexContentType;

        private static readonly Regex RegexBoundary;

        private static readonly Regex RegexCharset;

        private static readonly Regex RegexContentTransferEncodingBase64;

        private static readonly string[] TowNewLineDivision;

        private string m_Text;

        private string m_Boundary;

        private string m_ExtensionName;

        static MultipartRelatedExtractor()
        {
            RegexContentType = new Regex("Content-Type:(?<value>.*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            RegexBoundary = new Regex("boundary=\"(?<value>.*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            RegexCharset = new Regex("charset=\"(?<value>.*)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            RegexContentTransferEncodingBase64 = new Regex("Content-Transfer-Encoding\\s*:\\s*base64", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            TowNewLineDivision = new[] { string.Concat(Environment.NewLine, Environment.NewLine) };
        }

        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            m_Text = Encoding.UTF8.GetString(data);
            string[] strArrays = m_Text.Split(TowNewLineDivision, StringSplitOptions.RemoveEmptyEntries);
            if (strArrays.Length < 2)
            {
                LoggerWrapper.Logger.Warn("MultipartRelatedExtractor不支持当前文档");
                return false;
            }
            string regexValue = GetRegexValue(RegexContentType, strArrays[0]);
            m_Boundary = GetRegexValue(RegexBoundary, strArrays[0]);
            if (!regexValue.IsNullOrWhiteSpace() && regexValue.Trim().Equals("multipart/related", StringComparison.OrdinalIgnoreCase) && !m_Boundary.IsNullOrWhiteSpace())
            {
                return true;
            }
            LoggerWrapper.Logger.Warn("MultipartRelatedExtractor不支持当前文档(2)");
            return false;
        }

        private string ContentExtract(string content)
        {
            string[] strArrays = content.Split(TowNewLineDivision, StringSplitOptions.RemoveEmptyEntries);
            if (strArrays.Length < 2)
            {
                LoggerWrapper.Logger.Warn("MultipartRelatedExtractor不支持当前文档(3)");
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 1; i < strArrays.Length; i++)
            {
                stringBuilder.Append(strArrays[i]);
                stringBuilder.Append(Environment.NewLine);
            }
            string str = stringBuilder.ToString();
            if (RegexContentTransferEncodingBase64.IsMatch(strArrays[0]))
            {
                byte[] numArray = Convert.FromBase64String(str.Trim());
                string regexValue = GetRegexValue(RegexCharset, strArrays[0]);
                str = (!regexValue.IsNullOrWhiteSpace() ? Encoding.GetEncoding(regexValue).GetString(numArray) : (new TextExtractor()).Extract(m_ExtensionName, numArray, ExtractOption.Text).Text);
            }
            return HtmlExtractor.HtmlToText(str);
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            m_ExtensionName = extensionName;
            m_Boundary = string.Concat("--", m_Boundary);
            int num = m_Text.IndexOf(m_Boundary);
            int num1 = m_Text.IndexOf(m_Boundary, num + m_Boundary.Length);
            string str = m_Text.Substring(num + m_Boundary.Length, num1 - (num + m_Boundary.Length));
            return ContentExtract(str);
        }

        private string GetRegexValue(Regex regex, string[] lines)
        {
            string[] strArrays = lines;
            foreach (string t in strArrays)
            {
                Match match = regex.Match(t);
                if (match.Success)
                {
                    return match.Groups["value"].Value;
                }
            }
            return string.Empty;
        }

        private string GetRegexValue(Regex regex, string content)
        {
            Match match = regex.Match(content);
            if (!match.Success)
            {
                return string.Empty;
            }
            return match.Groups["value"].Value;
        }

        private bool IsTransferEncodingBase64(string content)
        {
            return RegexContentTransferEncodingBase64.IsMatch(content);
        }
    }
}