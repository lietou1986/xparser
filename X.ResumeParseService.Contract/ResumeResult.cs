using X.ResumeParseService.Contract.Models;

namespace X.ResumeParseService.Contract
{
    public class ResumeResult : ResumePredictResult
    {
        public ResumeData ResumeInfo { get; set; }
    }
}