using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class ProfessionalSkillData
    {
        public string SkillDesc { get; set; }
        public string Proficiency { get; set; }
        public string Months { get; set; }
        public string SourceText { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}