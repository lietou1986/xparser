using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class PracticalExperienceData
    {
        public string PracticeTitle { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string PracticeDesc { get; set; }
        public string Loacation { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}