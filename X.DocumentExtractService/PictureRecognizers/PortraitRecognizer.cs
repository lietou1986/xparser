using System.Configuration;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.PictureRecognizers
{
    public class PortraitRecognizer : IPictureRecognizer
    {
        private static readonly int MinWidth;

        private static readonly int MinHeight;

        private static readonly double MinRateHeightDivideWidth;

        private static readonly double MaxRateHeightDivideWidth;

        static PortraitRecognizer()
        {
            MinWidth = int.Parse(ConfigurationManager.AppSettings["Portrait:MinWidth"]);
            MinHeight = int.Parse(ConfigurationManager.AppSettings["Portrait:MinHeight"]);
            MinRateHeightDivideWidth = double.Parse(ConfigurationManager.AppSettings["Portrait:MinRateHeightDivideWidth"]);
            MaxRateHeightDivideWidth = double.Parse(ConfigurationManager.AppSettings["Portrait:MaxRateHeightDivideWidth"]);
        }

        protected bool IsValidPortraitBySize(double width, double height)
        {
            if (width > height || width == 0 || height == 0 || width > height || width < MinWidth || height < MinHeight)
            {
                return false;
            }
            double num = height / width;
            if (num >= MinRateHeightDivideWidth && num <= MaxRateHeightDivideWidth)
            {
                return true;
            }
            return false;
        }

        public PictureCategory Recongize(Picture picture)
        {
            PictureCategory pictureCategory = PictureCategory.Unknown;
            if (IsValidPortraitBySize(picture.Width, picture.Height))
            {
                pictureCategory = PictureCategory.Portrait;
            }
            return pictureCategory;
        }
    }
}