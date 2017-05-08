using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class ProjectExperienceData
    {
        public string ProjectTitle { get; set; }
        public string StartTime { get; set; }
        public string SoftwareEnvir { get; set; }
        public string HardEnvir { get; set; }
        public string DevelopTool { get; set; }
        public string EndTime { get; set; }
        public string ResponsibleFor { get; set; }
        public string ProjectDesc { get; set; }
        public string Company { get; set; }
        public string PositionTitle { get; set; }
        public string ProjectPerformance { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}