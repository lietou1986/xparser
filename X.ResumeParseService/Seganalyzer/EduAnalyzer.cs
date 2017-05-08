using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using X.ResumeParseService.Configuration;
using X.ResumeParseService.Contract.Models;
using X.ResumeParseService.Models;
using X.ResumeParseService.Utils;

namespace X.ResumeParseService.Seganalyzer
{
    public class EduAnalyzer
    {
        private List<string> resumeContentList = new List<string>();
        private ResumeData resumedata = new ResumeData();
        private List<SectionInfo> sectionInfoList = new List<SectionInfo>(); // 该段落是通过连续两行空行来分割的

        public EduAnalyzer(List<string> resumeContentList, List<SectionInfo> sectionInfoList)
        {
            this.resumeContentList = resumeContentList;
            this.sectionInfoList = sectionInfoList;
        }

        public List<EducationExperienceData> extractEducationExperience(int start, int end)
        {
            // 先遍历整个教育经历，统计出教育经历数量并获得每段教育经历的字符串，后针对每段教育经历进行分析，
            // 获取到教育经历中的开始结束时间，学校，专业和学历
            List<EducationExperienceData> eduExperienceDataList = new List<EducationExperienceData>();
            // string eduContent = "";

            List<string> schoolLineList = new List<string>(); // 用户保存每份教育经历的学校所在行，用于提取专业
            List<string> eduContentList = new List<string>();

            // 统计个数
            int count = 0;
            string content = "";
            for (int i = start; i < end; i++)
            {
                string line = resumeContentList[i];
                schoolLineList.Add(line);

                // 非特定词组开始的学校
                string pattern_school = "(?!于|在|.*在学校|.*是学校|.*的学校|.*毕业学校|.*我学校|.*全国中学|.*所在大学|.*所在高中|.*所在中学|.*所在初中|.*所在学校|.*学历学校|.*就读学校|.*就读大学|.*就读初中|.*就读高中|.*就读中学)([\u4e00-\u9fa5]{2,18}?)(学院|大学|学校|研究生院|中学)\\s*";
                // string pattern_school =
                // "(?!于|在|.*在学校|.*是学校|.*的学校|.*毕业学校|.*我学校|.*全国中学|.*所在大学|.*所在高中|.*所在中学|.*所在初中|.*所在学校|.*学历学校|.*就读学校|.*就读大学|.*就读初中|.*就读高中|.*就读中学)([\u4e00-\u9fa5])(大学|学院|学校|研究生院|中学)\\s*";

                var pattern = new Regex(pattern_school);
                var matcher = pattern.Match(line);
                // 匹配学校名&验证学校名合法性，string school = matcher.Groups[1].Value +
                // matcher.Groups[2].Value;
                if (matcher.Success && verifySchoolName(matcher.Groups[1].Value + matcher.Groups[2].Value))
                {
                    if (count > 0)
                    {
                        eduContentList.Add(content);
                        content = line;
                    }
                    else
                    {
                        content = content + " ### " + line;
                    }
                    count++;
                    EducationExperienceData eduExperienceData = new EducationExperienceData();
                    string school = matcher.Groups[1].Value + matcher.Groups[2].Value;
                    eduExperienceData.School = school;
                    eduExperienceDataList.Add(eduExperienceData);

                    if (i == end - 1)
                    {
                        eduContentList.Add(content);
                    }
                    continue;
                }
                else
                {
                    content = content + " ### " + line;
                    if (i == end - 1)
                    {
                        eduContentList.Add(content);
                    }
                }
            }

            if (count > 0)
            {
                for (int j = 0; j < eduContentList.Count; j++)
                {
                    string eduContent = eduContentList[j];
                    string degree = extractDegree(eduContent);
                    string major = extractMajor(eduContent);
                    string startTime = "";
                    string endTime = "";
                    string school = eduExperienceDataList[j].School;

                    var pattern_time = new Regex("((((19[6789][0-9]|20[01][0-9])\\s*(年|/|[.]|-|—|–))(\\s*(1[02]|[0]?[123456789])\\s*(月|/|[.]|-|—|–)?)(\\s*(3[01]|[12][0-9]|[0]?[1-9])(\\s*日)?)?)|(19[6789][0-9]|20[01][0-9])|([0-9]{2}\\s*年)(\\s*(1[02]|[0]?[123456789])\\s*月)?)"
                            + "\\s*((至\\s*今|现\\s*在|\\s*今)|((\\s|-|—|~|–|～|至|到)+)\\s*"
                            + "((((19[6789][0-9]|20[01][0-9])\\s*(年|/|[.]|-|—|–))(\\s*(1[02]|[0]?[123456789])\\s*(月|/|[.]|-|—|–)?)(\\s*(3[01]|[12][0-9]|[0]?[1-9])(\\s*日)?)?)|(19[6789][0-9]|20[01][0-9])|([0-9]{2}\\s*年)(\\s*(1[02]|[0]?[123456789])\\s*月)?|至\\s*今|现\\s*在|\\s*今))");
                    var matcher = pattern_time.Match(eduContent);
                    if (matcher.Success)
                    {
                        // 抽取日期合法性判定
                        string[] items = Regex.Split(matcher.Groups[0].Value.Trim(), "\\D");
                        bool valid_date_format = true;
                        foreach (string item in items)
                            // 判断日期子项长度
                            if (item.Trim().Length == 3 || item.Trim().Length > 4)
                            {
                                valid_date_format = false;
                                break;
                            }

                        if (!valid_date_format)
                            continue;

                        // date format normalization
                        // startTime = matcher.Groups[1].Value;
                        string[] date_start_items = Regex.Split(matcher.Groups[1].Value.Trim(), "\\D");
                        foreach (string item in date_start_items)
                            if (item.Trim() != "")
                            {
                                if (startTime == "")
                                    startTime = item;
                                else
                                    startTime += ("-" + item);
                            }

                        // date format normalization
                        // endTime = matcher.group(11).replace("至", "");
                        string[] date_end_items = Regex.Split(matcher.Groups[16].Value.Trim(), "\\D");
                        foreach (string item in date_end_items)
                            if (item.Trim() != "")
                            {
                                if (endTime == "")
                                    endTime = item;
                                else
                                    endTime += ("-" + item);
                            }

                        eduExperienceDataList[j].StartTime = startTime;
                        eduExperienceDataList[j].EndTime = endTime;
                        eduExperienceDataList[j].Completable = true;
                    }
                    else
                    {
                        // 召回时间，应对时间位于学校上一行情况
                        /*
                         * 学习经历 ： 1995.09 - 1997.03 \n### 泰山科技学院 \n ###1998.09 -
                         * 2001.07\n###河南医科大学
                         */
                        if (j > 0)
                        {
                            string[] front_edu_content_array = eduContentList[j - 1].Split(new string[] { "###" }, StringSplitOptions.None);

                            string recall_edu_timeStr = front_edu_content_array.Length > 1
                                    ? front_edu_content_array[front_edu_content_array.Length - 1] : eduContentList[j];

                            string pattern_school_str = "(?!于|在|.*在学校|.*是学校|.*的学校|.*我学校|全国中学|所在大学|所在高中|所在中学|所在初中|所在学校|.*学历学校|就读学校|就读大学|就读初中|就读高中|就读中学)([\u4e00-\u9fa5]{2,18}?)(学院|大学|学校|研究生院|中学)\\s*";
                            var pattern_school = new Regex(pattern_school_str);

                            matcher = pattern_school.Match(recall_edu_timeStr);
                            //
                            if (!matcher.Success)
                            {
                                matcher = pattern_time.Match(recall_edu_timeStr);
                                if (matcher.Success)
                                {
                                    // date format normalization
                                    // startTime = matcher.Groups[1].Value;
                                    startTime = DateTools.dateFormat(matcher.Groups[1].Value.Trim());
                                    // endTime = matcher.group(11).replace("至", "");
                                    endTime = DateTools.dateFormat(matcher.Groups[16].Value.Trim());

                                    eduExperienceDataList[j].StartTime = startTime;
                                    eduExperienceDataList[j].EndTime = endTime;
                                    eduExperienceDataList[j].Completable = true;
                                }
                            }
                        }
                    }

                    eduExperienceDataList[j].Degree = degree;
                    eduExperienceDataList[j].Major = major;
                }
            }

            List<EducationExperienceData> eduExperienceDataList_final = new List<EducationExperienceData>();

            // 按开始时间升序重排序
            eduExperienceDataList.Sort();
            // 有效性检测
            foreach (EducationExperienceData item in eduExperienceDataList)
            {
                // step1:教育经历项>1时，过滤掉没有时间属性的教育经历子项
                if (eduExperienceDataList.Count > 1 && (!item.Completable))
                    continue;

                if (eduExperienceDataList_final.Count == 0)
                {
                    eduExperienceDataList_final.Add(item);
                    continue;
                }

                // step2:
                if (duplicationCheck(item, eduExperienceDataList_final))
                    eduExperienceDataList_final.Add(item);
            }

            // 有效性检测召回,例如教育子项都没有时间属性
            if (eduExperienceDataList_final.Count < 1 && eduExperienceDataList.Count > 0)
                eduExperienceDataList_final.Add(eduExperienceDataList[0]);

            return eduExperienceDataList_final;
        }

        /*
         * 验证学校名合法性，召回与准确率是一对矛盾体，使用学校列表提高准确度的同时会降低召回率
         */

        private bool verifySchoolName(string schoolName)
        {
            if (ResourcesConfig.universitySet.Contains(schoolName))
                return true;

            return false;
        }

        private bool verifyMajor(string majorName)
        {
            if (ResourcesConfig.majorList.Contains(majorName))
                return true;

            return false;
        }

        // 学习经历重复时间检测
        private bool duplicationCheck(EducationExperienceData eduItem, List<EducationExperienceData> eduList)
        {
            // 较晚教育经历子项的开始时间应》=之前一项教育经历子项的结束时间年份

            int currentStartData = Convert.ToInt32(
                    (eduItem.StartTime.Trim() == "" ? "0" : Regex.Split(eduItem.StartTime, "年|/|[.]|-|—")[0].Trim()));

            int currentEndData = currentStartData;
            if (eduItem.EndTime != null && eduItem.EndTime.Trim() == "至今")
                currentEndData = DateTime.Now.Year;
            try
            {
                currentEndData = Convert.ToInt32(
                        (eduItem.EndTime.Trim() == "" ? "0" : Regex.Split(eduItem.EndTime, "年|/|[.]|-|—")[0].Trim()));
            }
            catch (Exception ex)
            {
            }

            EducationExperienceData maxEduData = eduList[eduList.Count - 1];

            int MaxStartData = Convert.ToInt32(
                    (maxEduData.StartTime.Trim() == "" ? "0" : Regex.Split(maxEduData.StartTime, "年|/|[.]|-|—")[0].Trim()));

            int MaxEndData = MaxStartData;
            if (eduItem.EndTime != null && maxEduData.EndTime.Trim() == "至今")
                MaxEndData = DateTime.Now.Year;
            try
            {
                MaxEndData = Convert.ToInt32(
                        (maxEduData.EndTime.Trim() == "" ? "0" : Regex.Split(maxEduData.EndTime, "年|/|[.]|-|—")[0].Trim()));
            }
            catch (Exception ex)
            {
            }

            // 较晚教育经历子项的开始时间应》=之前一项教育经历子项的结束时间
            if (currentStartData >= MaxEndData && currentEndData >= MaxEndData)
                return true;
            else
                return false;
        }

        public static EducationExperienceData getHighestEduExperience(List<EducationExperienceData> eduList)
        {
            eduList.Sort();

            return eduList[eduList.Count - 1];
        }

        public string extractDegree(string content)
        {
            // 获取学历
            string degree = "";
            var pattern = new Regex("(" + ResourcesConfig.degreeRegex + ")");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                degree = matcher.Groups[1].Value;
            }
            return degree;
        }

        public string extractMajor(string content)
        {
            string major = "";
            string[] items = Regex.Split(content, "[^\u4e00-\u9fa5]");

            // 学习经历子项按非汉字切分，判断是否为专业名称
            foreach (string item in items)
                if ((item.Trim() != "") && ResourcesConfig.majorList.Contains(item.Trim()))
                    return item.Trim();

            // 使用专业集合正则式匹配
            string pattern_name = "(" + ResourcesConfig.majorRegex + ")";
            var pattern = new Regex(pattern_name);
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                string match = matcher.Groups[1].Value;
                major = match;
                if (verifyMajor(major))
                    return major;
            }

            pattern = new Regex("(#|\\s|所\\s*学\\s*)专\\s*业[^\u4e00-\u9fa5]*([\u4e00-\u9fa5]+)[^\u4e00-\u9fa5]+");
            matcher = pattern.Match(content);
            if (matcher.Success)
            {
                major = matcher.Groups[2].Value;
                if (verifyMajor(major))
                    return major;
            }
            if (major == "")
            {
                pattern = new Regex("(^|#|\\s)(([\u4e00-\u9fa5]+))专\\s*业");
                matcher = pattern.Match(content);
                if (matcher.Success)
                {
                    major = matcher.Groups[2].Value;
                    if (verifyMajor(major))
                        return major;
                }
            }

            return "";
        }

        public List<EducationExperienceData> searchEducationExperience()
        {
            // 从每个段中去搜索教育经历
            List<EducationExperienceData> eduExperienceDataList = new List<EducationExperienceData>();
            for (int i = 0; i < sectionInfoList.Count; i++)
            {
                SectionInfo sectionInfo = sectionInfoList[i];
                eduExperienceDataList = extractEducationExperience(sectionInfo.Start, sectionInfo.End);
                if (eduExperienceDataList.Count > 0)
                {
                    break;
                }
            }
            return eduExperienceDataList;
        }
    }
}