using Newtonsoft.Json;
using System.Collections.Generic;
using X.DocumentExtractService.Contract.Models;

namespace X.ResumeParseService.Contract.Models
{
    public class ResumeData
    {
        public ResumeLanguage ResumeLanguage { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string IdentityID { get; set; }
        public string Email { get; set; }
        public string QQ { get; set; }
        public string HouseHolds { get; set; }
        public string Residence { get; set; }
        public string PoliticalLandscape { get; set; }
        public string MaritalStatus { get; set; }
        public string LatestDegree { get; set; }
        public string LatestSchool { get; set; }
        public string LatestMajor { get; set; }
        public string LatestCompanyName { get; set; }
        public string LatestPositionTitle { get; set; }
        public string LatestSalary { get; set; }
        public string LatestIndustry { get; set; }
        public EducationExperienceData HighestEducation { get; set; }
        public WorkExperienceData CurrentWork { get; set; }
        public string CurrentSalary { get; set; }
        public int WorkYears { get; set; }
        public JobTarget JobTarget { get; set; }
        public List<EducationExperienceData> EducationExperience { get; set; }
        public List<WorkExperienceData> WorkExperience { get; set; }
        public List<TrainingExperienceData> TrainingExperience { get; set; }
        public List<LanguageSkillData> LanguageSkill { get; set; }
        public List<CertificateData> Certficate { get; set; }
        public List<ProjectExperienceData> ProjectExperience { get; set; }
        public List<ProfessionalSkillData> ProfessionalSkill { get; set; }
        public List<PracticalExperienceData> PracticalExperience { get; set; }
        public StudyInfoData StudyInfo { get; set; }
        public string SelfEvaluation { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 判断简历是否有效
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Name)) return false;
            if (string.IsNullOrEmpty(Phone) && string.IsNullOrEmpty(Email)) return false;
            if (EducationExperience == null || EducationExperience.Count == 0) return false;
            if (WorkExperience == null || WorkExperience.Count == 0) return false;
            return true;
        }
    }
}