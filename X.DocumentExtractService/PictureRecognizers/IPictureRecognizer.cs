using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.PictureRecognizers
{
    public interface IPictureRecognizer
    {
        PictureCategory Recongize(Picture picture);
    }
}