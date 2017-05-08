using Dorado.Core;
using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using X.ResumeParseService.Configuration;
using X.ResumeParseService.Contract.Models;
using X.ResumeParseService.Models;
using X.ResumeParseService.Seganalyzer;

namespace X.ResumeParseService
{
    /**
     * 1. 先对文本进行分段，在分段完成后对各个段进行解析，
     * 目前主要对基本信息，工作经历，项目经历，教育经验和专业技能进行解析，其他段由于差距很多，暂不支持解析
     *
     * 2. 如果通过上述方法还是没有分析出教育经历和工作经历则通过全文匹配方式来获取工作经验，
     * 和教育经验（项目经验过于灵活，暂时不通过全文匹配方式来获取）
     *
     */

    public class TextCVParser
    {
        private string ResumeText { get; set; }

        private ResumeData resumedata = new ResumeData();
        private Dictionary<string, SectionInfo> segmentMap = new Dictionary<string, SectionInfo>(); // 用于保存有段名的段落情况
        private List<SectionInfo> sectionInfoList = new List<SectionInfo>(); // 该段落是通过连续两行空行来分割的
        private Dictionary<string, string> segmentTextMap = new Dictionary<string, string>();
        private List<string> resumeContentList = new List<string>();

        public ResumeData Parse()
        {
            try
            {
                // pre-process
                preProcess();
                // building education info
                extractEducationExperience();
                // building basic info
                extractBasicInfo();

                // add highest edu-info into basic info
                if (resumedata.LatestSchool == null || resumedata.LatestSchool.Trim() == "")
                {
                    if (resumedata.EducationExperience.Count > 0)
                    {
                        EducationExperienceData edu_item = EduAnalyzer
                                .getHighestEduExperience(resumedata.EducationExperience);
                        resumedata.LatestSchool = edu_item.School;
                        resumedata.LatestDegree = edu_item.Degree;
                        resumedata.LatestMajor = edu_item.Major;
                    }
                }

                // build work info
                extractWorkExperience();

                WorkExperienceData workData = WorkAnalyzer.getearliestWorkExperience(resumedata.WorkExperience);
                if (workData != null && workData.StartTime != "")
                {
                    int workYears = 0;
                    if (workData.StartTime != "")
                    {
                        int workStartYear = Convert.ToInt32(workData.StartTime.Trim().Split(new char[] { '-' })[0]);

                        workYears = DateTime.Now.Year - workStartYear;
                        resumedata.WorkYears = workYears;
                    }
                }

                // build except job info
                extractJobTarget();
                // build self-evaluation info
                extractSelfEvaluation();
                // building language-skill info
                extractLanguageSkill();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("简历解析错误", ex);
            }
            return resumedata;
        }

        public string getContent(int start, int end)
        {
            string content = "";
            string line = "";
            for (int i = start; i < end; i++)
            {
                line = resumeContentList[i];
                if (content == "")
                {
                    content = line;
                }
                else
                {
                    content = content + "\r\n" + line;
                }
            }
            return content;
        }

        public void extractBasicInfo()
        {
            if (segmentMap.ContainsKey("basic_info"))
            {
                SectionInfo sectionInfo = segmentMap["basic_info"];
                BasicInfoAnalyzer analyzer = new BasicInfoAnalyzer(resumeContentList, resumedata);
                analyzer.extractBasicInfo(sectionInfo.Start, sectionInfo.End);
            }
        }

        public void extractSelfEvaluation()
        {
            string seg_name = "self_evaluation";

            if (segmentMap.ContainsKey(seg_name))
            {
                SelfEvaluationAnalyzer analyzer = new SelfEvaluationAnalyzer(resumeContentList);
                SectionInfo sectionInfo = segmentMap[seg_name];
                string selfEvaluation = analyzer.extractSelfEvaluation(sectionInfo.Start, sectionInfo.End);
                resumedata.SelfEvaluation = selfEvaluation;
            }
        }

        public void extractJobTarget()
        {
            string seg_name = "career_objective";
            if (segmentMap.ContainsKey(seg_name))
            {
                TargetJobAnalyzer analyzer = new TargetJobAnalyzer(resumeContentList);
                SectionInfo sectionInfo = segmentMap[seg_name];
                JobTarget jobTarget = analyzer.extractJobTarget(sectionInfo.Start, sectionInfo.End);
                resumedata.JobTarget = jobTarget;
            }
            else
            {
                TargetJobAnalyzer analyzer = new TargetJobAnalyzer(resumeContentList);
                JobTarget jobTarget = analyzer.searchJobTarget();
                resumedata.JobTarget = jobTarget;
            }
        }

        public void extractWorkExperience()
        {
            string seg_name = "work_experience";
            WorkAnalyzer analyzer = new WorkAnalyzer(resumeContentList, sectionInfoList);

            if (segmentMap.ContainsKey(seg_name))
            {
                SectionInfo sectionInfo = segmentMap[seg_name];
                List<WorkExperienceData> workExperienceDataList = analyzer.extractWorkExperience(sectionInfo.Start,
                        sectionInfo.End);
                resumedata.WorkExperience = workExperienceDataList;
            }
            else
            {
                // 从个人基本信息召回
                if (segmentMap.ContainsKey("basic_info"))
                {
                    SectionInfo sectionInfo = segmentMap["basic_info"];
                    List<WorkExperienceData> workExperienceDataList = analyzer.searchWorkExperience(sectionInfo.Start,
                            sectionInfo.End);
                    // 判断单位是否与学校重复
                    if (this.resumedata.LatestSchool != null && workExperienceDataList.Count >= 1 && (this.resumedata
                            .LatestSchool.Trim() != workExperienceDataList[0].CompanyName.Trim()))
                    {
                        resumedata.WorkExperience = workExperienceDataList;
                    }
                }
            }
        }

        public void extractEducationExperience()
        {
            string seg_name = "edu_background";
            if (segmentMap.ContainsKey(seg_name))
            {
                SectionInfo sectionInfo = segmentMap[seg_name];
                EduAnalyzer analyzer = new EduAnalyzer(resumeContentList, sectionInfoList);

                List<EducationExperienceData> educationExperienceDataList = analyzer
                        .extractEducationExperience(sectionInfo.Start, sectionInfo.End);
                resumedata.EducationExperience = educationExperienceDataList;
            }
            else
            {
                EduAnalyzer analyzer = new EduAnalyzer(resumeContentList, sectionInfoList);
                List<EducationExperienceData> educationExperienceDataList = analyzer.searchEducationExperience();
                resumedata.EducationExperience = educationExperienceDataList;
            }
        }

        // TODO:未完成语言抽取
        public void extractLanguageSkill()
        {
            string seg_name = "language";
            if (segmentMap.ContainsKey(seg_name))
            {
                SectionInfo sectionInfo = segmentMap[seg_name];

                List<LanguageSkillData> languageSkill = new List<LanguageSkillData>();
                string pattern_lang = "英\\s*语\\s*四\\s*级|英\\s*语\\s*六\\s*级|英\\s*语\\s*专\\s*业\\s*四\\s*级|专\\s*业\\s*八\\s*级|四级|六级|CET4|CET6|TEM4|TEM8|CET-4|CET-6|TEM-4|TEM-8";

                string line = "";
                for (int i = sectionInfo.Start; i < sectionInfo.End; i++)
                {
                    line = resumeContentList[i].Trim();
                    var pattern = new Regex(pattern_lang);
                    var matcher = pattern.Match(line);
                    if (matcher.Success)
                    {
                        LanguageSkillData langData = new LanguageSkillData();
                        i += 3;
                        languageSkill.Add(langData);
                    }
                }
            }
            else
            {
                List<LanguageSkillData> languageSkill = new List<LanguageSkillData>();
                string pattern_lan = "英\\s*语\\s*四\\s*级|英\\s*语\\s*六\\s*级|英\\s*语\\s*专\\s*业\\s*四\\s*级|专\\s*业\\s*八\\s*级|四级|六级|CET4|CET6|TEM4|TEM8|CET-4|CET-6|TEM-4|TEM-8";

                string line = resumeContentList.ToString();
                var pattern = new Regex(pattern_lan);
                var matcher = pattern.Match(line);
                if (matcher.Success)
                {
                    LanguageSkillData langData = new LanguageSkillData();
                    languageSkill.Add(langData);
                }
            }
        }

        public void preProcess()
        {
            ResourcesConfig.Load();

            string[] datas = ResumeText.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string n in datas)
            {
                string line = n;
                if ((line.Contains("前程无忧") || line.Contains("智联招聘") || line.Contains("猎聘网")))
                    continue;
                line = Regex.Replace(line, "[\\u00A0]+", " ");
                line = Regex.Replace(line, "[\\\u3000]+", " ");
                line = line.Replace("http://my.51job.com", " ").Replace("此简历来自猎聘网", " ").Replace("Liepin.com ", " ").Replace("最大的中文高端招聘社区", " ");
                line = line.Replace("：", ":").Trim();
                if (!line.IsNullOrWhiteSpace())
                    resumeContentList.Add(line);
            }

            // building segmentIndexMap info
            this.segmentMap = SegmentSplit.GetSegments(resumeContentList);

            foreach (var v in segmentMap)
            {
                string key = v.Key;
                SectionInfo sectionInfo = v.Value;

                string content = getContent(sectionInfo.Start, sectionInfo.End);
                this.segmentTextMap[key] = content;

                this.sectionInfoList.Add(sectionInfo);
            }
        }

        public TextCVParser(string resumeText)
        {
            this.ResumeText = resumeText;
        }
    }
}