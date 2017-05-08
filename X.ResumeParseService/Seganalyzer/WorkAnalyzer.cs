using System.Collections.Generic;
using System.Text.RegularExpressions;
using X.ResumeParseService.Contract.Models;
using X.ResumeParseService.Models;
using X.ResumeParseService.Utils;

namespace X.ResumeParseService.Seganalyzer
{
    /**
     * 工作项解析器，显式“工作经历”，目前只提供工作时间，工作名称，职位，工作描述四项。 非显式“工作经历”只提供工作名称及职位两项。
     *
     * @author haiming.yin
     *
     */

    public class WorkAnalyzer
    {
        private List<string> resumeContentList = new List<string>();
        private List<SectionInfo> sectionInfoList = new List<SectionInfo>(); // 该段落是通过连续两行空行来分割的

        public WorkAnalyzer(List<string> resumeContentList, List<SectionInfo> sectionInfoList)
        {
            this.resumeContentList = resumeContentList;
            this.sectionInfoList = sectionInfoList;
        }

        private bool containsWorkExperience(List<WorkExperienceData> workExperienceDataList,
                WorkExperienceData currentWorkExp)
        {
            foreach (WorkExperienceData workexp in workExperienceDataList)
            {
                if (workexp.StartTime == currentWorkExp.StartTime
                        && workexp.EndTime == currentWorkExp.EndTime)
                    return true;
            }
            return false;
        }

        public List<WorkExperienceData> extractWorkExperience(int start, int end)
        {
            // isWorkSection 用于当前分析的段是否为工作经历段落
            List<WorkExperienceData> workExperienceDataList = new List<WorkExperienceData>();
            List<string> workContentList = new List<string>();

            // 按行匹配，通过工作时间部分统计工作经验个数
            int workCount = 0;
            string subWorkContent = "";
            for (int i = start; i < end; i++)
            {
                string line = resumeContentList[i];

                var pattern = new Regex(
                        "((((19[6789][0-9]|20[01][0-9])\\s*(年|/|[.]|-|—|–))(\\s*(1[02]|[0]?[123456789])\\s*(月|/|[.]|-|—|–)?)(\\s*(3[01]|[12][0-9]|[0]?[1-9])(\\s*日)?)?)|(19[6789][0-9]|20[01][0-9])|([0-9]{2}\\s*年)(\\s*(1[02]|[0]?[123456789])\\s*月)?)"
                                + "\\s*((至\\s*今|现\\s*在|\\s*今)|((\\s|-|—|~|–|～|至|到)+)\\s*"
                                + "((((19[6789][0-9]|20[01][0-9])\\s*(年|/|[.]|-|—|–))(\\s*(1[02]|[0]?[123456789])\\s*(月|/|[.]|-|—|–)?)(\\s*(3[01]|[12][0-9]|[0]?[1-9])(\\s*日)?)?)|(19[6789][0-9]|20[01][0-9])|([0-9]{2}\\s*年)(\\s*(1[02]|[0]?[123456789])\\s*月)?|至\\s*今|现\\s*在|\\s*今))");
                var matcher = pattern.Match(line);
                if (matcher.Success)
                {
                    // 抽取日期合法性判定
                    string[] items = Regex.Split(matcher.Groups[0].Value.Trim(), "\\D");
                    bool valid_date_format = true;
                    foreach (string item in items)
                        if (item.Trim().Length == 3 || item.Trim().Length > 4)
                        {
                            valid_date_format = false;
                            break;
                        }
                    if (!valid_date_format)
                        continue;

                    WorkExperienceData workExperienceData = new WorkExperienceData();
                    string startTime = DateTools.dateFormat(matcher.Groups[1].Value);
                    string endTime = DateTools.dateFormat(matcher.Groups[16].Value);

                    workExperienceData.StartTime = startTime;
                    workExperienceData.EndTime = endTime;

                    // 判断是否已存在工作时间段子项，假设同一时间段只做一份工
                    /*
                     * JR126243590R90000000000.pdf 2011.04-至今 中安网脉(北京)技术股份有限公司 公司行业：
                     * 计算机硬件/网络设备 研发经理 2011.04-至今 所在地区： 北京 下属人数： 14 人 工作职责： 1. 开发
                     * PCI-E 卡算法加速模块并集成到基于 x86 工控机的 IPSec VPN 系统中，执行管理部 门的测试和检验流程。
                     */
                    if (containsWorkExperience(workExperienceDataList, workExperienceData))
                    {
                        subWorkContent = subWorkContent + "###" + line;
                        // if (i == end - 1) {
                        // workContentList.Add(subWorkContent);
                        // }

                        // subWorkContent="";
                        continue;
                    }

                    workExperienceDataList.Add(workExperienceData);
                    if (workCount > 0)
                    {
                        workContentList.Add(subWorkContent);
                        subWorkContent = line;
                    }
                    else
                    {
                        subWorkContent = line;
                    }

                    workCount++;
                    continue;
                }
                else
                {
                    subWorkContent = subWorkContent + "###" + line;
                }
            }

            // 添加最后一项工作子项的工作内容
            workContentList.Add(subWorkContent);

            // 包含工作经历时间项
            if (workCount > 0)
            {
                for (int j = 0; j < workExperienceDataList.Count; j++)
                {
                    string line = workContentList[j];
                    
                    string positionTitle = extractPosition(line);
                    string companyName = extractCompany_fuzzy(line);

                    workExperienceDataList[j].CompanyName = companyName;
                    workExperienceDataList[j].PositionTitle = positionTitle;

                    string jobDesc = line;
                    workExperienceDataList[j].JobDesc = jobDesc;
                }

                return workExperienceDataList;
            }

            // 未包含工作经历时间部分
            if (workCount == 0)
            {
                workExperienceDataList = searchWorkExperience(start, end);
            }

            return workExperienceDataList;
        }

        public List<WorkExperienceData> searchWorkExperience(int start, int end)
        {
            List<WorkExperienceData> workExperienceDataList = new List<WorkExperienceData>();
            // 抽取公司名称
            for (int i = start; i < end; i++)
            {
                string line = resumeContentList[i];

                string companyName = extractCompany_exact(line);
                if (companyName != null && companyName != "")
                {
                    WorkExperienceData workExperienceData = new WorkExperienceData();
                    workExperienceData.CompanyName = companyName;
                    workExperienceDataList.Add(workExperienceData);
                    // 全局模糊搜索到一个公司即可
                    return workExperienceDataList;
                }
            }

            return workExperienceDataList;
        }

        // 提取公司
        public string extractCompany_exact(string line)
        {
            string company = "";

            var pattern = new Regex("(公\\s*司)(:|:)(([\u4e00-\u9fa5]|[a-z0-9])+)($|#|\\s)",
                    RegexOptions.IgnoreCase);
            var matcher = pattern.Match(line);
            if (matcher.Success)
            {
                company = matcher.Groups[3].Value;
                return company;
            }

            pattern = new Regex(
                    "(?!于|在|#)[\u4e00-\u9fa5]+?[\u4e00-\u9fa5|(|)|0-9|（|）]{2,15}(公司|集团|代表处|办事处|营业部|经营部|事务所|学校|中心|研究所|研究院|酒店|商行|工作室|银行|俱乐部|加盟店|集团|门诊部)+",
                    RegexOptions.IgnoreCase);
            matcher = pattern.Match(line);
            if (matcher.Success)
            {
                company = matcher.Groups[0].Value;
                return company;
            }

            List<string> companies = CompanyTokenAnalyzer.segment(line);
            if (companies.Count > 0)
                return companies[0];
            return company;
        }

        // 全文中提取公司
        public string extractCompany_fuzzy(string line)
        {
            line = line.ToLower();
            string company = "";

            var pattern = new Regex("(公\\s*司)(:|:)(([\u4e00-\u9fa5]|[a-z0-9])+)($|#|\\s)",
                    RegexOptions.IgnoreCase);
            var matcher = pattern.Match(line);
            if (matcher.Success)
            {
                company = matcher.Groups[3].Value;
                return company;
            }

            // TODO:交付时顺序后移到正则之后做召回 find maximum length position
            List<string> companies = CompanyTokenAnalyzer.segment(line);
            if (companies.Count > 0)
                return companies[0].ToUpper();

            pattern = new Regex(
                    "(?!于|在|#)[\u4e00-\u9fa5]+?[\u4e00-\u9fa5|(|)|0-9|（|）]{2,20}(公司|集团|机构|代表处|办事处|营业部|经营部|事务所|学校|中心|研究所|研究院|酒店|商行|工作室|银行|俱乐部|加盟店|集团|门诊部)",
                    RegexOptions.IgnoreCase);
            matcher = pattern.Match(line);
            if (matcher.Success)
            {
                company = matcher.Groups[0].Value;
                return company;
            }

            return company;
        }

        // 提取职位
        public string extractPosition(string line)
        {
            string position = "";
            // 先寻找是否有关键字，如果有关键字”职位“ 则直接提取后面的词并返回
            var pattern = new Regex("(职\\s*位|角\\s*色)(:|:)?(([\u4e00-\u9fa5]|[a-z0-9])+)($|#|\\s)",
                    RegexOptions.IgnoreCase);
            var matcher = pattern.Match(line);
            if (matcher.Success)
            {
                position = matcher.Groups[3].Value;
                return position;
            }

            // // 如果没有关键词”职位“，则直接通过找职位字典做最大匹配

            List<string> positions = PositionTokenAnalyzer.segment(line);
            // find maximum length position
            int max_index = 0;
            int max_length = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].Length > max_length)
                {
                    max_length = positions[i].Length;
                    max_index = i;
                }
            }
            if (positions.Count > 0)
                // return positions.get(max_index);
                return positions[0];

            return position;
        }

        public static WorkExperienceData getLastestWorkExperience(List<WorkExperienceData> workList)
        {
            workList.Sort();
            return workList[workList.Count - 1];
        }

        public static WorkExperienceData getearliestWorkExperience(List<WorkExperienceData> workList)
        {
            if (workList == null)
                return null;
            workList.Sort();
            return workList[0];
        }
    }
}