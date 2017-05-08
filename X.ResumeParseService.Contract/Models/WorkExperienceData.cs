using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace X.ResumeParseService.Contract.Models
{
    public class WorkExperienceData : IComparable<WorkExperienceData>
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Druation { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCatagory { get; set; }
        public string CompanyScale { get; set; }
        public string IndustryCatagory { get; set; }
        public string PositionCatagory { get; set; }
        public string PositionTitle { get; set; }
        public string Salary { get; set; }
        public string JobDesc { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string CompanyDesc { get; set; }
        public List<ManageExperienceData> ManageExperienceList { get; set; }

        public int CompareTo(WorkExperienceData work)
        {
            try
            {
                if (work.StartTime == "" || this.StartTime == "" || work.StartTime == null
                      || this.StartTime == null)
                {
                    return 0;
                }
                string currentStartData = (this.StartTime.Trim() == "" ? "0"
                        : Regex.Split(this.StartTime, "年|/|[.]|-|—")[0].Trim());
                string inputStartData = (work.StartTime.Trim() == "" ? "0"
                        : Regex.Split(work.StartTime, "年|/|[.]|-|—")[0].Trim());

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