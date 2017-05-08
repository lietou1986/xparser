using System.Collections.Generic;

namespace X.ResumeParseService.Seganalyzer
{
    public class SelfEvaluationAnalyzer
    {
        private List<string> resumeContentList = new List<string>();

        public SelfEvaluationAnalyzer(List<string> resumeContentList)
        {
            this.resumeContentList = resumeContentList;
        }

        public string extractSelfEvaluation(int start, int end)
        {
            string selfEvaluation = "";
            string line = "";
            for (int i = start; i < end; i++)
            {
                line = resumeContentList[i];
                if (selfEvaluation == "")
                {
                    selfEvaluation = line;
                }
                else
                {
                    selfEvaluation = selfEvaluation + "\r\n" + line;
                }
            }
            return selfEvaluation;
        }
    }
}