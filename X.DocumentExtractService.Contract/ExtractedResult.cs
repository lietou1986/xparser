using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Contract
{
    public class ExtractedResult
    {
        /// <summary>
        /// ·µ»ØÍ¼Æ¬Êý¾Ý
        /// </summary>
        public Picture[] Images
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }
    }
}