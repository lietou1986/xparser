using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class TrainingExperienceData
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Instituation { get; set; }
        public string Location { get; set; }
        public string Course { get; set; }
        public string Certificate { get; set; }
        public string TrainDesc { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}