using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class OtherInfoData
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}