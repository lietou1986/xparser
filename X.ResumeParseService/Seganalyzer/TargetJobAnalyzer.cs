using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using X.ResumeParseService.Contract.Models;

namespace X.ResumeParseService.Seganalyzer
{
    /**
     * 1.求职意向解析器，“求职意向”段内提供模糊匹配， 1.1按行做匹配 1.2按行未匹配成功，按段召回匹配 2.若不存在“求职意向”段则使用全局精确匹配。
     *
     * @author haiming.yin
     *
     */

    public class TargetJobAnalyzer
    {
        private List<string> resumeContentList = new List<string>();

        public TargetJobAnalyzer(List<string> resumeContentList)
        {
            this.resumeContentList = resumeContentList;
        }

        /*
         * 提供段内“求职意向”按行匹配结果
         */

        public JobTarget extractJobTarget(int start, int end)
        {
            JobTarget jobTarget = new JobTarget();

            //获取段内所有数据
            string line = getContent(start, end);

            // 行业
            string industry = getIndustry(line, false);
            if (industry != null)
                jobTarget.JobIndustry = industry;

            // 职位
            string jobCareer = getJobCareer(line, false);
            if (jobCareer != null)
                jobTarget.JobCareer = jobCareer;

            // 薪资
            string salary = getSalary(line, false);
            if (salary != null)
                jobTarget.Salary = salary;

            // 地点
            string location = getLocation(line, false);
            if (location != null)
                jobTarget.JobLocation = location;

            // 性质
            string jobCatagory = getJobCatagory(line, false);
            if (jobCatagory != null)
                jobTarget.JobCatagory = jobCatagory;

            // 到岗时间
            string enrollTime = getEnrollTime(line, false);
            if (enrollTime != null)
                jobTarget.EnrollTime = enrollTime;
            return jobTarget;
        }

        /*
         * 提供文本全局匹配“求职意向”，前缀使用精确匹配(意向行业)，段内可以使用模糊匹配（行业，例如51job的简历）
         */

        public JobTarget extractJobTarget(string resumeStr)
        {
            JobTarget jobTarget = new JobTarget();

            // resumeFullStr="期望月薪 6001-8000元/月 目前状况： 应届毕业生";
            // resumeFullStr = "期望月薪 :面议6001-8000元/月 目前状况： 应届毕业生";

            // 行业
            string industry = getIndustry(resumeStr, true);
            if (industry != null)
                jobTarget.JobIndustry = industry;

            // 职位
            string jobCareer = getJobCareer(resumeStr, true);
            if (jobCareer != null)
                jobTarget.JobCareer = jobCareer;

            // 薪资
            string salary = getSalary(resumeStr, true);
            if (salary != null)
                jobTarget.Salary = salary;

            // 地点
            string location = getLocation(resumeStr, true);
            if (location != null)
                jobTarget.JobLocation = location;

            // 性质
            string jobCatagory = getJobCatagory(resumeStr, true);
            if (jobCatagory != null)
                jobTarget.JobCatagory = jobCatagory;

            // 到岗时间
            string enrollTime = getEnrollTime(resumeStr, true);
            if (enrollTime != null)
                jobTarget.EnrollTime = enrollTime;

            return jobTarget;
        }

        private string getLocation(string resumeStr, bool exact)
        {
            string exact_Pattern = "(意\\s*向\\s*地\\s*区|意\\s*向\\s*工\\s*作\\s*地|期\\s*望\\s*工\\s*作\\s*地\\s*区|期\\s*望\\s*工\\s*作\\s*地\\s*点|目\\s*标\\s*地\\s*点|希\\s*望\\s*地\\s*点|希\\s*望\\s*工\\s*作\\s*地|目\\s*标\\s*工\\s*作\\s*地|目\\s*标\\s*地\\s*区|希\\s*望\\s*地\\s*区|意\\s*向\\s*地\\s*点|目\\s*标\\s*城\\s*市)([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5]+\\s)";
            string fuzzy_Pattern = "(意\\s*向\\s*地\\s*区|意\\s*向\\s*工\\s*作\\s*地|期\\s*望\\s*工\\s*作\\s*地\\s*区|期\\s*望\\s*工\\s*作\\s*地\\s*点|目\\s*标\\s*地\\s*点|希\\s*望\\s*地\\s*点|希\\s*望\\s*工\\s*作\\s*地|目\\s*标\\s*工\\s*作\\s*地|目\\s*标\\s*地\\s*区|希\\s*望\\s*地\\s*区|意\\s*向\\s*地\\s*点|目\\s*标\\s*城\\s*市|期\\s*望\\s*地\\s*点|工\\s*作\\s*地\\s*点|地\\s*点)([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5|、|，|,]+\\s*)";

            var pattern = new Regex(exact ? exact_Pattern : fuzzy_Pattern);
            var matcher = pattern.Match(resumeStr);
            if (matcher.Success)
            {
                return matcher.Groups[3].Value;
            }

            return null;
        }

        private string getJobCareer(string resumeStr, bool exact)
        {
            string exact_Pattern = "(意\\s*向\\s*岗\\s*位|意\\s*向\\s*职\\s*位|期\\s*望\\s*岗\\s*位|期\\s*望\\s*职\\s*位|希\\s*望\\s*岗\\s*位|目\\s*标\\s*岗\\s*位|"
                    + "目\\s*标\\s*职\\s*位|希\\s*望\\s*职\\s*位|目\\s*标\\s*职\\s*能|期\\s*望\\s*从\\s*事\\s*职\\s*业|期\\s*望\\s*职\\s*业|目\\s*标\\s*职\\s*务)([^\u4e00-\u9fa5]*)(.*?\\s)";
            string fuzzy_Pattern = "(意\\s*向\\s*岗\\s*位|意\\s*向\\s*职\\s*位|期\\s*望\\s*岗\\s*位|期\\s*望\\s*职\\s*位|希\\s*望\\s*岗\\s*位|目\\s*标\\s*岗\\s*位|"
                    + "目\\s*标\\s*职\\s*位|希\\s*望\\s*职\\s*位|目\\s*标\\s*职\\s*能|期\\s*望\\s*从\\s*事\\s*职\\s*业|期\\s*望\\s*职\\s*业|目\\s*标\\s*职\\s*务|职\\s*能|职\\s*位\\s*名\\s*称|应\\s*聘\\s*职\\s*位|期\\s*望\\s*工\\s*作[：|:])([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5|/|、|，|a-z]+\\s*)";

            var pattern = new Regex(exact ? exact_Pattern : fuzzy_Pattern);
            var matcher = pattern.Match(resumeStr);
            if (matcher.Success)
            {
                return matcher.Groups[3].Value;
            }

            return null;
        }

        private string getSalary(string resumeStr, bool exact)
        {
            string exact_Pattern = "(期\\s*望\\s*薪\\s*资|期\\s*望\\s*薪\\s*水|期\\s*望\\s*月\\s*薪|期\\s*望\\s*工\\s*资|希\\s*望\\s*薪\\s*水|目\\s*标\\s*薪\\s*水|目\\s*标\\s*工\\s*资|希\\s*望\\s*工\\s*资|"
                    + "希\\s*望\\s*月\\s*薪|目\\s*标\\s*月\\s*薪)(\\D*)((.*?面议)|(.*?/月)|(.*?月)|(.*?/年)|(.*?年)|(\\d{1,}k?-*\\d{1,}k?))";
            string fuzzy_Pattern = "(期\\s*望\\s*薪\\s*资|期\\s*望\\s*薪\\s*水|期\\s*望\\s*月\\s*薪|期\\s*望\\s*工\\s*资|希\\s*望\\s*薪\\s*水|目\\s*标\\s*薪\\s*水|目\\s*标\\s*工\\s*资|希\\s*望\\s*工\\s*资|"
                    + "希\\s*望\\s*月\\s*薪|目\\s*标\\s*月\\s*薪)(\\D*)((.*?面议)|(.*?/月)|(.*?月)|(.*?/年)|(.*?年)|(\\d{1,}k?-*\\d{1,}k?))";

            var pattern = new Regex(exact ? exact_Pattern : fuzzy_Pattern);
            var matcher = pattern.Match(resumeStr);
            if (matcher.Success)
            {
                return matcher.Groups[3].Value;
            }

            return null;
        }

        private string getIndustry(string resumeStr, bool exact)
        {
            string exact_Pattern = "(意\\s*向\\s*行\\s*业|期\\s*望\\s*从\\s*事\\s*行\\s*业|期\\s*望\\s*行\\s*业|希\\s*望\\s*行\\s*业|目\\s*标\\s*行\\s*业)([^\u4e00-\u9fa5|a-z]*)(.*?\\s)";
            string fuzzy_Pattern = "(意\\s*向\\s*行\\s*业|期\\s*望\\s*从\\s*事\\s*行\\s*业|期\\s*望\\s*行\\s*业|希\\s*望\\s*行\\s*业|目\\s*标\\s*行\\s*业|行\\s*业)([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5|/|、|，|a-z]+\\s*)";

            var pattern = new Regex(exact ? exact_Pattern : fuzzy_Pattern, RegexOptions.IgnoreCase);
            var matcher = pattern.Match(resumeStr);
            if (matcher.Success)
            {
                return matcher.Groups[3].Value;
            }

            return "";
        }

        private string getJobCatagory(string resumeStr, bool exact)
        {
            string exact_Pattern = "(意\\s*向\\s*工\\s*作\\s*类\\s*型|期\\s*望\\s*工\\s*作\\s*类\\s*型|期\\s*望\\s*工\\s*作\\s*性\\s*质|意\\s*向\\s*工\\s*作\\s*性\\s*质|目\\s*标\\s*工\\s*作\\s*性\\s*质|目\\s*标\\s*工\\s*作\\s*类\\s*型|工\\s*作\\s*性\\s*质)([^\u4e00-\u9fa5]*)(.*?\\s)";
            string fuzzy_Pattern = "(意\\s*向\\s*工\\s*作\\s*类\\s*型|期\\s*望\\s*工\\s*作\\s*类\\s*型|期\\s*望\\s*工\\s*作\\s*性\\s*质|意\\s*向\\s*工\\s*作\\s*性\\s*质|目\\s*标\\s*工\\s*作\\s*性\\s*质|目\\s*标\\s*工\\s*作\\s*类\\s*型|工\\s*作\\s*类\\s*型|工\\s*作\\s*性\\s*质|求\\s*职\\s*性\\s*质)([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5|/]+\\s*)";

            var pattern = new Regex(exact ? exact_Pattern : fuzzy_Pattern);
            var matcher = pattern.Match(resumeStr);
            if (matcher.Success)
            {
                return matcher.Groups[3].Value;
            }

            return null;
        }

        private string getEnrollTime(string resumeStr, bool exact)
        {
            string exact_Pattern = "(预\\s*计\\s*到\\s*岗\\s*时\\s*间|可\\s*到\\s*岗\\s*时\\s*间|到\\s*岗\\s*时\\s*间)([^\u4e00-\u9fa5|\\d]*)(.*?\\s)";
            string fuzzy_Pattern = "(预\\s*计\\s*到\\s*岗\\s*时\\s*间|可\\s*到\\s*岗\\s*时\\s*间|到\\s*岗\\s*时\\s*间)([^\u4e00-\u9fa5|\\d]*)(.*?\\s)";

            var pattern = new Regex(exact ? exact_Pattern : fuzzy_Pattern);
            var matcher = pattern.Match(resumeStr);
            if (matcher.Success)
            {
                return matcher.Groups[3].Value;
            }

            return null;
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
                    content = content + "  " + line;
                }
            }
            return content;
        }

        public JobTarget searchJobTarget()
        {
            // 从每个段中去搜索求职意向
            JobTarget jobTarget = null;
            StringBuilder resumeTxtBuilder = new StringBuilder();
            foreach (string line in resumeContentList)
                resumeTxtBuilder.Append(line + "  ");

            jobTarget = extractJobTarget(resumeTxtBuilder.ToString());

            return jobTarget;
        }
    }
}