using Newtonsoft.Json;
using System.Collections.Generic;

namespace X.ResumeParseService.Contract.Models
{
    public class StudyInfoData
    {
        public List<string> ScholarShipList { get; set; }
        public string ActivityDesc { get; set; }
        public List<RewardData> RewardDataList { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}