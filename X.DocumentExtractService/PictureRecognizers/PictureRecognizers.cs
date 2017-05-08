using System.Linq;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.PictureRecognizers
{
    public static class PictureRecognizers
    {
        private static IPictureRecognizer[] m_Recognizers;

        static PictureRecognizers()
        {
            m_Recognizers = new IPictureRecognizer[] { new PortraitRecognizer() };
        }

        public static PictureCategory GetPictureType(Picture picture)
        {
            IPictureRecognizer[] mRecognizers = m_Recognizers;
            return mRecognizers.Select(t => t.Recongize(picture)).FirstOrDefault(pictureCategory => pictureCategory != PictureCategory.Unknown);
        }
    }
}