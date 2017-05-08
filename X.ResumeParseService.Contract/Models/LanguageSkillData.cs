using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class LanguageSkillData
    {
        public string Catagory { get; set; }
        public string ReadAndWriteAbility { get; set; }
        public string ListenAndSpeakAbility { get; set; }
        public string Level { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}