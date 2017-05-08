using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using X.ResumeParseService.Configuration;
using X.ResumeParseService.Contract.Models;
using X.ResumeParseService.Utils;

namespace X.ResumeParseService.Seganalyzer
{
    public class BasicInfoAnalyzer
    {
        private List<string> resumeContentList = new List<string>();
        private ResumeData resumedata = new ResumeData();

        public BasicInfoAnalyzer(List<string> resumeContentList, ResumeData resumedata)
        {
            this.resumeContentList = resumeContentList;
            this.resumedata = resumedata;
        }

        public void extractBasicInfo(int start, int end)
        {
            string basicInfo = "";

            for (int i = start; i < end; i++)
            {
                string line = resumeContentList[i];
                if (line.Trim() != "")
                {
                    if (basicInfo == "")
                    {
                        basicInfo = line;
                        continue;
                    }
                    basicInfo = basicInfo + " ### " + line;
                }
            }

            // 过滤噪音名
            string name = extractName(basicInfo);
            resumedata.Name = name;

            string email = extractEmail(basicInfo);
            resumedata.Email = (email);

            string identityID = extractIdentityID(basicInfo);
            resumedata.IdentityID = (identityID);

            string phone = extractPhone(basicInfo);
            resumedata.Phone = (phone);

            string qq = extractQQ(basicInfo);
            resumedata.QQ = (qq);

            string gender = extractGender(basicInfo);
            resumedata.Gender = (gender);

            string birthDay = extractBirthday(basicInfo);
            resumedata.Birthday = (birthDay);

            int age = 0;
            if (birthDay.Trim() != "")
            {
                age = DateTime.Now.Year - Convert.ToInt32(birthDay.Trim().SubString(0, 4));
                resumedata.Age = (age);
            }
            else
            {
                age = extractAge(basicInfo);
                resumedata.Age = (age);
            }

            var pattern = new Regex("(未婚|已婚)", RegexOptions.IgnoreCase );
            var matcher = pattern.Match(basicInfo);
            if (matcher.Success)
            {
                string maritalStatus = matcher.Groups[1].Value;
                resumedata.MaritalStatus = (maritalStatus);
            }

            string houseHolds = extractHouseHolds(basicInfo);
            resumedata.HouseHolds = (houseHolds);

            string residence = extractResidence(basicInfo);
            resumedata.Residence = (residence);
        }

        public string extractName(string content)
        {
            string name = "";
            // 预处理
            content = Regex.Replace(content, ResourcesConfig.namefadeRegex, "#").Replace(" ", "");

            // "姓\\s*名.*?((" +
            // ResourcesConfig.nameStartRegex+")(\\s*[\u4e00-\u9fa5]){1,2})\\s*(应\\s*聘|求\\s*职|性\\s*别|籍\\s*贯|$|\\s)"
            // 格式:姓名+非汉字+姓+汉字2-3位+非汉字
            string pattern_name = "姓\\s*名[^\u4e00-\u9fa5]+((" + ResourcesConfig.nameStartRegex
                    + ")([\u4e00-\u9fa5]{1,2}))([^\u4e00-\u9fa5]|#)?";
            if (resumedata.Name == null)
            {
                var p = new Regex(pattern_name);
                var m = p.Match(content);
                while (m.Success)
                {
                    string match = m.Groups[1].Value;
                    if (match.Length > 1 && match.Length <= 4)
                    {
                        name = match;
                        return name;
                    }
                    m = m.NextMatch();
                }
            }

            // 没有显式“名字”字段 做召回处理
            // filter noise

            string nameRegex = "((" + ResourcesConfig.nameStartRegex + ")([\u4e00-\u9fa5]{1,2}))([^\u4e00-\u9fa5]|#)?";
            var pattern = new Regex(nameRegex);
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                // 如果满足第一种方式则直接取出
                return matcher.Groups[1].Value;
            }

            return name;
        }

        public string extractPhone(string content)
        {
            // 提取手机号码， 手机号码之间可能有空格
            string phone = "";
            var pattern = new Regex(
                    "(((\\d{3,4}-)\\d{7,8})|(1[3589]\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d))(\\D|$)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                phone = matcher.Groups[1].Value;
                phone = phone.Replace(" ", "");
                if (phone.IndexOf("-") >= 0)
                {
                    resumedata.Phone = (phone);
                }
                else
                {
                    if (phone.Length == 11)
                    {
                        resumedata.Phone = (phone);
                    }
                }
            }
            return phone;
        }

        public string extractQQ(string content)
        {
            string qq = "";
            var pattern = new Regex("qq(:|:)?\\s*(\\d{5,11})", RegexOptions.IgnoreCase );
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                qq = matcher.Groups[2].Value;
            }
            return qq;
        }

        public string extractBirthday(string content)
        {
            // 提取生日
            string birthDay = "";
            // Pattern
            // pattern=new Regex("([1][09][6789][0-9](.*?\\d+.*?\\d+\\s*(日)?|.*?\\d+\\s*(月)?|\\s*(年)?))(###|\\s|$)");
            var pattern = new Regex("(([1][9][6789][0-9]\\s*(年|/|.|-))(\\s*\\d+\\s*(月|/|.|-))?(\\s*\\d+(\\s*日)?)?)\\s*");

            // Pattern
            // pattern=new Regex("([1][09][6789][0-9](.*?\\d+\\s*(月|/|.|-)?.*?\\d+\\s*(日)?|.*?\\d+\\s*(月|/|.|-)?\\s*\\d+|.*?\\d+\\s*(月)?|.*?\\d+|.*?\\d+(月)?|年)?)(###|\\s|$)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                birthDay = matcher.Groups[1].Value;
                birthDay = birthDay != null ? DateTools.dateFormat(birthDay) : "";
            }
            return birthDay;
        }

        public string extractEmail(string content)
        {
            string email = "";
            var pattern = new Regex("(email|电子邮件|邮箱).*?(:)?(\\S+@\\S)(\\s|$)", RegexOptions.IgnoreCase );

            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                email = matcher.Groups[1].Value;
                return email;
            }

            // Pattern
            // pattern=new Regex("([a-z0-9]*[.-_]?[a-z0-9]+)*@([a-z0-9]*[-_.]?[a-z0-9]+)+[.][a-z]{2,3}([.][a-z]{2})?",RegexOptions.IgnoreCase);
            pattern = new Regex("([-_.a-z0-9]+@[-_.a-z0-9]+)", RegexOptions.IgnoreCase );

            matcher = pattern.Match(content);
            if (matcher.Success)
            {
                email = matcher.Groups[1].Value;
            }
            return email;
        }

        public int extractAge(string content)
        {
            int age = 0;
            var pattern = new Regex("(\\d{2}\\s*)(岁)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                age = Convert.ToInt32(matcher.Groups[1].Value.Trim());
                return age;
            }

            // TODO:召回年龄，使用生日，身份证等推断出
            if (age == 0)
            {
                pattern = new Regex("(年\\s*龄|年\\s*纪)\\D*(\\d{2})");
                matcher = pattern.Match(content);
                if (matcher.Success)
                {
                    age = Convert.ToInt32(matcher.Groups[2].Value);
                }
            }
            return age;
        }

        public string extractIdentityID(string content)
        {
            // 获取身份证号码
            string identityID = "";
            var pattern = new Regex("(\\d{15}$|\\d{18}|\\d{17}(\\d|X|x))");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                identityID = matcher.Groups[1].Value;
            }
            return identityID;
        }

        public string extractGender(string content)
        {
            string gender = "";
            var pattern = new Regex("性.*?别.*?(男|女)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                gender = matcher.Groups[1].Value;
                return gender;
            }

            if (gender == "")
            {
                pattern = new Regex("(男|女)");
                matcher = pattern.Match(content);
                if (matcher.Success)
                {
                    gender = matcher.Groups[1].Value;
                }
            }
            return gender;
        }

        public string extractSchool(string content)
        {
            // 获取毕业学院。学校信息可能出现在基本信息也可能出现在教育经历中
            string school = "";
            var pattern = new Regex("(" + ResourcesConfig.universityRegex + ")(\\s|$)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                school = matcher.Groups[1].Value;
            }
            if (school == "")
            {
                // pattern = new Regex("([\u4e00-\u9fa5]+(学院|学校|大学|中学|小学))");
                pattern = new Regex("[^\\s,.:;，。：；—-]{1,14}(?:大学|学院|学校|研究生院)");
                matcher = pattern.Match(content);
                if (matcher.Success)
                {
                    school = matcher.Groups[0].Value;
                }
            }

            return school;
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

            var pattern = new Regex("(^|\\s)专\\s*业(:|:|\\s+)([\u4e00-\u9fa5|[a-z]]+)($|\\s[^\u4e00-\u9fa5])");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                major = matcher.Groups[3].Value;
            }
            if (major == "")
            {
                pattern = new Regex("(^|\\s)(([\u4e00-\u9fa5]+))专\\s*业");
                matcher = pattern.Match(content);
                if (matcher.Success)
                {
                    major = matcher.Groups[2].Value;
                }
            }

            return major;
        }

        public string extractHouseHolds(string content)
        {
            string houseHolds = "";
            var pattern = new Regex(
                    "(户\\s*籍\\s*所\\s*在\\s*地|户\\s*籍|户\\s*口\\s*所\\s*在(?:\\s*区\\s*县|地\\s*地\\s*址|地\\s*址|地)|户\\s*口\\s*(:|：)|籍 \\s*贯\\s*(:|：|：))([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5|\\d]+\\s)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                houseHolds = matcher.Groups[5].Value;
            }

            return houseHolds;
        }

        public string extractResidence(string content)
        {
            string residence = "";
            var pattern = new Regex(
                    "(现\\s*居\\s*住\\s*于|现\\s*居\\s*住\\s*地\\s*地\\s*址|现\\s*居\\s*住\\s*地\\s*|家\\s*庭\\s*住\\s*址\\s*[：|:]|居\\s*住\\s*地\\s*[：|:])([^\u4e00-\u9fa5]*)([\u4e00-\u9fa5|\\d]+\\s)");
            var matcher = pattern.Match(content);
            if (matcher.Success)
            {
                residence = matcher.Groups[3].Value;
            }

            return residence;
        }
    }
}