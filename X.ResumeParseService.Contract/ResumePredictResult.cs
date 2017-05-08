using X.DocumentExtractService.Contract;

namespace X.ResumeParseService.Contract
{
    public class ResumePredictResult : ExtractedResult
    {
        public int Score
        {
            get;
            set;
        }

        public bool IsResume
        {
            get;
            set;
        }
    }
}