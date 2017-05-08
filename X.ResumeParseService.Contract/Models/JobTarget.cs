using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class JobTarget
    {
        public string JobCatagory { get; set; }
        public string JobLocation { get; set; }
        public string JobCareer { get; set; }
        public string JobIndustry { get; set; }
        public string Salary { get; set; }
        public string Status { get; set; }
        public string EnrollTime { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}