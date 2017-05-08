using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using X.DocumentExtractService.Contract.Models;
using SPdfDocument = Spire.Pdf.PdfDocument;

namespace X.DocumentExtractService.Extractor
{
    internal class SpirePdfExtractor : DocumentExtractor
    {
        protected override bool CanBeExtracted(string extensionName, byte[] data)
        {
            if (!extensionName.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("不支持当前文档");
            }
            return true;
        }

        protected override Picture[] ExtractImages(string extensionName, byte[] data)
        {
            List<Picture> pictures = new List<Picture>();
            PdfDocumentBase doc = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    doc = SPdfDocument.MergeFiles(new Stream[] { memoryStream });
                    foreach (PdfPageBase page in doc.Pages)
                    {
                        foreach (Image image in page.ExtractImages())
                        {
                            pictures.Add(new Picture()
                            {
                                Data = GetImageData(image),
                                Extension = ImageFormat.Jpeg.ToString(),
                                Width = image.Width,
                                Height = image.Height
                            });
                        }
                    }
                }
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close();
                }
            }
            return pictures.ToArray();
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            StringBuilder stringBuilder = new StringBuilder();

            PdfDocumentBase doc = null;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    doc = SPdfDocument.MergeFiles(new Stream[] { memoryStream });
                    foreach (PdfPageBase page in doc.Pages)
                    {
                        stringBuilder.AppendLine(page.ExtractText());
                    }
                    doc.Close();
                }
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close();
                }
            }
            return stringBuilder.ToString();
        }

        private byte[] GetImageData(Image img)
        {
            byte[] array;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, ImageFormat.Jpeg);
                memoryStream.Position = 0;
                array = memoryStream.ToArray();
            }
            return array;
        }
    }
}