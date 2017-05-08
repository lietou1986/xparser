using Dorado.Extensions;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace X.ResumeParseService.Contract.Models
{
    public class EducationExperienceData : IComparable
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string School { get; set; }
        public string Major { get; set; }
        public string Degree { get; set; }
        public string SeriesIncurs { get; set; }

        [JsonIgnore]
        public bool Completable { get; set; }

        public int CompareTo(object obj)
        {
            try
            {
                EducationExperienceData s = (EducationExperienceData)obj;
                string currentStartData = StartTime.IsNullOrWhiteSpace() ? "0"
                        : Regex.Split(StartTime.Trim(), "年|/|[.]|-|—")[0].Trim();
                string inputStartData = s.StartTime.IsNullOrWhiteSpace() ? "0" : Regex.Split(s.StartTime.Trim(), "年|/|[.]|-|—")[0].Trim();

                int currentStart = Convert.ToInt32(currentStartData);
                int inputStart = Convert.ToInt32(inputStartData);
                if (currentStart > inputStart)
                {
                    return 1;
                }
                else if (currentStart == inputStart)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex) { return 0; }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}