using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class ManageExperienceData
    {
        public string ReportTo { get; set; }
        public string SubordinatesNum { get; set; }
        public string Suborinates { get; set; }
        public string Reterence { get; set; }
        public string LeavingReason { get; set; }
        public string KeyPerformance { get; set; }
        public string OverseasWorkExperience { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}