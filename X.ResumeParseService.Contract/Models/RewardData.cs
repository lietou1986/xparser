using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class RewardData
    {
        public string Rewards { get; set; }
        public string RewardsLevel { get; set; }
        public string Time { get; set; }
        public string Desc { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}