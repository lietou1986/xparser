using Dorado.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace X.ResumeParseService
{
    /// <summary>
    /// 简历识别器
    /// </summary>
    public class ResumeChecker
    {
        private static readonly Dictionary<string, int> PatternMap = new Dictionary<string, int>();

        static ResumeChecker()
        {
            // general info
            PatternMap["的\\s*简\\s*历|\\s*简\\s*历"] = 20;
            PatternMap["个人基本信息|基本信息|个人信息"] = 10;
            PatternMap["职业目标|职业规划|职业意向|求职意向|期望工作"] = 30;
            PatternMap["受教育经历|教育经历|受教育背景|教育背景|受教育状况|教育状况|受教育情况|教育情况|学习经历|教育"] = 10;
            PatternMap["工作经验|工作经历|工作背景|任职情况"] = 20;
            PatternMap["实习经历|实习"] = 10;
            PatternMap["自我评价|自我评估|个人特长|个人评价|自我描述|自我介绍|个人总结|自我简评|个性特点|背景概要"] = 10;
            PatternMap["培训经历|专业培训|培训"] = 10;
            PatternMap["技能专长|技能特长|专业技能|IT技能|技能|专业特长|职业技能与特长"] = 10;
            PatternMap["语言能力"] = 10;
            PatternMap["获得证书|获得认证|证书认证|证书|认证"] = 10;

            // details info
            PatternMap["男|女"] = 5;
            PatternMap["已婚|未婚"] = 5;

            PatternMap["(((\\d{3,4}-)\\d{7,8})|(1[35789]\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d\\s*\\d))(\\D|$)"] = 5;
            PatternMap["[_a-z0-9-]+(?:\\.[_a-z0-9-]+)*@[a-z0-9-]+(?:\\.[a-z0-9-]+)*(?:\\.[a-z]{2,3})"] = 5;

            PatternMap["期望工资|期望月薪|期望年薪|期望薪水|期望起薪|期望底薪|期望收入"] = 15;
            PatternMap["全职|兼职|实习"] = 5;

            PatternMap["本科|学士|大专|专科|硕士|博士|中专|初中|高中|研究生|MBA|EMBA|博士后"] = 5;
            PatternMap["(?<=^|[\\s:：])[^\\s,.:;，。：；—-]{2,14}(?:大学([\\W|_])|学院([\\W|_])|学校([\\W|_])|研究生院([\\W|_]))"] = 5;
            PatternMap["英语四级|英语六级|专业四级|专业八级|四级|六级|CET-4|CET-6|TEM-4|TEM-8"] = 5;

            PatternMap["((\\d{1})|(\\d{2}))(年)*(经验|工作经验)"] = 5;
            PatternMap["(?<=^|[\\s:：])[^:：—;；。，,\\s-]{2,20}(?:公司([\\W|_])|代表处([\\W|_])|办事处([\\W|_])|营业部([\\W|_])|经营部([\\W|_])|事务所([\\W|_])|学校([\\W|_])|中心([\\W|_])|研究所([\\W|_])|研究院([\\W|_])|大酒店([\\W|_])|商行([\\W|_])|工作室([\\W|_])|银行([\\W|_])|幼儿园([\\W|_])|俱乐部([\\W|_])|厂([\\W|_])|加盟店([\\W|_])|集团([\\W|_])|门诊部([\\W|_])|杂志社([\\W|_]))"] = 5;
        }

        private static int CountResumeScore(string resumeBody)
        {
            return (from var in PatternMap select var.Key into key let reg = new Regex(key, RegexOptions.Multiline ) let match = reg.Match(resumeBody) where match.Success select PatternMap[key]).Sum();
        }

        public static int Predict(string resumeBody)
        {
            try
            {
                resumeBody = Regex.Replace(resumeBody, "\\s", string.Empty).ToUpper();

                int fileLength = resumeBody.Length;

                // step1:简历长度判别
                if (fileLength < 100 || fileLength > 20000)
                    return 0;

                // step2:简历完整度判别
                return CountResumeScore(resumeBody);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("简历校验失败", ex);
                return 0;
            }
        }
    }
}