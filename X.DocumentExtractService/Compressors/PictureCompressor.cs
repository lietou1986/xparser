using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace X.DocumentExtractService.Compressors
{
    public abstract class PictureCompressor
    {
        protected byte[] m_Data;

        protected PictureCompressor(byte[] data)
        {
            m_Data = data;
        }

        public byte[] GetCompressedImageBytes(int maxWidth, int maxHeight)
        {
            if (m_Data == null || m_Data.Length == 0)
            {
                return null;
            }
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream(m_Data))
            {
                Image image = CompressImage(Image.FromStream(memoryStream), maxWidth, maxHeight);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    image.Save(memoryStream2, ImageFormat.Jpeg);
                    byte[] array = new byte[memoryStream2.Length];
                    memoryStream2.Position = 0L;
                    memoryStream2.Read(array, 0, (int)memoryStream2.Length);
                    result = array;
                }
            }
            return result;
        }

        protected abstract Image CompressImage(Image originalImage, int width, int height);

        protected virtual Size CalculateCompressedSize(int maxWidth, int maxHeight, int imageOriginalWidth, int imageOriginalHeight)
        {
            double num;
            double num2;
            if (imageOriginalWidth < maxWidth && imageOriginalHeight < maxHeight)
            {
                num = imageOriginalWidth;
                num2 = imageOriginalHeight;
            }
            else if (imageOriginalWidth * 1.0 / imageOriginalHeight > maxWidth * 1.0 / maxHeight)
            {
                num = maxWidth;
                num2 = num * imageOriginalHeight / imageOriginalWidth;
            }
            else
            {
                num2 = maxHeight;
                num = num2 * imageOriginalWidth / imageOriginalHeight;
            }
            return new Size(Convert.ToInt32(num), Convert.ToInt32(num2));
        }
    }
}