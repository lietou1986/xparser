using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class SkillData
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}