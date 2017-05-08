using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.IO;
using System.Text;

namespace X.DocumentExtractService.Extractor
{
    internal class PdfExtractor : DocumentExtractor
    {
        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            if (!extensionName.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("不支持当前文档");
            }
            return true;
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            PdfReader pdfReader = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    pdfReader = new PdfReader(memoryStream);
                    for (int num = 0; num < pdfReader.NumberOfPages; num++)
                    {
                        string textFromPage = PdfTextExtractor.GetTextFromPage(pdfReader, num + 1);
                        stringBuilder.AppendLine(textFromPage);
                    }
                }
            }
            finally
            {
                if (pdfReader != null)
                {
                    pdfReader.Close();
                }
            }
            return stringBuilder.ToString();
        }
    }
}