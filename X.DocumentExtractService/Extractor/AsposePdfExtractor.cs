using Aspose.Pdf;
using Aspose.Pdf.Text;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Extractor
{
    internal class AsposePdfExtractor : DocumentExtractor
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
            List<XImage> xImages = new List<XImage>();
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                foreach (Page page in (new Document(memoryStream)).Pages)
                {
                    if (page.Resources.Images.Count <= 0)
                    {
                        continue;
                    }
                    foreach (XImage image in page.Resources.Images)
                    {
                        xImages.Add(image);
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
            return pictures.ToArray();
        }

        protected override string ExtractText(string extensionName, byte[] data)
        {
            StringBuilder stringBuilder = new StringBuilder();

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                using (Document doc = new Document(memoryStream))
                {
                    TextAbsorber textAbsorber = new TextAbsorber();

                    doc.Pages.Accept(textAbsorber);

                    stringBuilder.Append(textAbsorber.Text);
                }
            }
            return Regex.Replace(stringBuilder.ToString(), "Evaluation Only. Created with Aspose[\\S|\\s]* Aspose Pty Ltd.", "");
        }

        private byte[] GetImageData(XImage img)
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