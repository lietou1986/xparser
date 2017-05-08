using System.Drawing;
using System.Drawing.Drawing2D;

namespace X.DocumentExtractService.Compressors
{
    public class HighQualityPictureCompressor : PictureCompressor
    {
        public HighQualityPictureCompressor(byte[] data)
            : base(data)
        {
        }

        protected override Image CompressImage(Image originalImage, int width, int height)
        {
            Image bitmap;
            Graphics graphic = null;
            try
            {
                var empty = CalculateCompressedSize(width, height, originalImage.Width, originalImage.Height);
                bitmap = new Bitmap(empty.Width, empty.Height);
                graphic = Graphics.FromImage(bitmap);
                graphic.CompositingQuality = CompositingQuality.HighQuality;
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.Clear(Color.Transparent);
                graphic.DrawImage(originalImage, new Rectangle(0, 0, empty.Width, empty.Height), new Rectangle(0, 0, originalImage.Width, originalImage.Height), GraphicsUnit.Pixel);
            }
            finally
            {
                if (graphic != null)
                {
                    graphic.Dispose();
                }
            }
            return bitmap;
        }
    }
}