using System.Collections.Generic;
using System.Text.RegularExpressions;
using X.ResumeParseService.Models;

namespace X.ResumeParseService
{
    public class SegmentSplit
    {
        private static Dictionary<string, string> pattern_seg_exact = new Dictionary<string, string>();

        static SegmentSplit()
        {
            // segment info(精确匹配）
            pattern_seg_exact["^个\\s*人\\s*基\\s*本\\s*信\\s*息\\s+|^基\\s*本\\s*信\\s*息\\s+|^个\\s*人\\s*信\\s*息\\s+"]="basic_info";
            pattern_seg_exact["^求\\s*职\\s*意\\s*向|^期\\s*望\\s*工\\s*作\\s+|^求\\s*职\\s*目\\s*标\\s+|^职\\s*业\\s*目\\s*标\\s+|^职\\s*业\\s*规\\s*划\\s+|^职\\s*业\\s*意\\s*向\\s+"]=
                    "career_objective";
            // 末尾添加“学习”独立行
            pattern_seg_exact["^受\\s*教\\s*育\\s*经\\s*历\\s+|^教\\s*育\\s*经\\s*历\\s+|^受\\s*教\\s*育\\s*背\\s*景\\s+|^教\\s*育\\s*背\\s*景\\s+|^受\\s*教\\s*育\\s*状\\s*况\\s+|^教\\s*育\\s*状\\s*况\\s+|^受\\s*教\\s*育\\s*情\\s*况\\s+|^教\\s*育\\s*情\\s*况\\s+|^学\\s*习\\s*经\\s*历\\s+|^教\\s*育\\s*培\\s*训\\s*$|^教育/工作经历|^学\\s*习\\s*生\\s*涯|^学\\s*习\\s*$"]=
                    "edu_background";
            pattern_seg_exact["^在\\s*校\\s*学\\s*习\\s*情\\s*况|^校\\s*园\\s*经\\s*历|^获\\s*奖\\s*情\\s*况"]="edu_presentation";

            // 末尾添加“工作”独立行
            pattern_seg_exact["^\\s*工作.*经历\\s*$|^工\\s*作\\s*经\\s*验\\s+|^个\\s*人\\s*工\\s*作\\s*经\\s*历\\s+|^工\\s*作\\s*经\\s*历\\s+|^工\\s*作\\s*背\\s*景\\s+|^社\\s*会\\s*经\\s*历\\s+|^社\\s*会\\s*经\\s*验\\s+|^任\\s*职\\s*情\\s*况\\s+|教育/工作经历|工作/项目经验|^个\\s*人\\s*经\\s*历\\s*$"]=
                    "work_experience";
            pattern_seg_exact["^项\\s*目\\s*经\\s*历\\s+|^项\\s*目\\s*经\\s*验\\s+|^项\\s*目\\s*介\\s*绍\\s+|工作/项目经验"]=
                    "project_experience";
            pattern_seg_exact["^实\\s*习\\s*经\\s*历\\s+|^实\\s*习\\s*经\\s*验\\s+|^在\\s*校\\s*实\\s*践\\s*经\\s*历\\s+|^在\\s*校\\s*实\\s*践\\s+|^社\\s*会\\s*实\\s*践\\s+|^社\\s*会\\s*实\\s*践\\s*经\\s*历"]=
                    "intern_experience";
            pattern_seg_exact["^自\\s*我\\s*评\\s*价\\s+|^自\\s*我\\s*评\\s*估\\s+|^个\\s*人\\s*评\\s*价\\s+|^自\\s*我\\s*描\\s*述\\s+|^自\\s*我\\s*介\\s*绍\\s+|^个\\s*人\\s*总\\s*结\\s+|^自\\s*我\\s*简\\s*评\\s+|^背\\s*景\\s*概\\s*要\\s+"]=
                    "self_evaluation";
            pattern_seg_exact["^培\\s*训\\s*经\\s*历\\s+|^培\\s*训\\s*情\\s*况\\s+"]="training_experience";
            pattern_seg_exact["^技\\s*能\\s*专\\s*长\\s+|^技\\s*能\\s*特\\s*长\\s+|^专\\s*业\\s*技\\s*能\\s+|^I\\s*T\\s*能\\s*力\\s+|^专\\s*业\\s*专\\s*长\\s+|^职\\s*业\\s*技\\s*能\\s*与\\s*专\\s*长\\s+|^个\\s*人\\s*技\\s*能\\s+|语言及IT技能"]=
                    "skill";
            pattern_seg_exact["^语\\s*言\\s*能\\s*力\\s+|^语\\s*言\\s*及\\s*I\\s*T\\s*能\\s*力\\s+"]="language";
            pattern_seg_exact["^获\\s*得\\s*证\\s*书\\s+|^获\\s*得\\s*认\\s*证\\s+|^证\\s*书\\s*认\\s*证\\s+|^证\\s*书\\s+"]=
                    "certificate";

        }

        public static Dictionary<string, SectionInfo> GetSegments(List<string> ResumeContentList)
        {
            Dictionary<string, SectionInfo> sectionMap = new Dictionary<string, SectionInfo>();

            string currentSection = "basic_info";
            SectionInfo sectionInfo = new SectionInfo(0, 0);
            sectionMap[currentSection] = sectionInfo;

            string currentSectionName = "basic_info";

            string line;
            for (int i = 0; i < ResumeContentList.Count; i++)
            {
                line = ResumeContentList[i];

                // 非中文字符空格化处理，且末位补一位空格
                string trimLine = Regex.Replace(line, "[^\u4e00-\u9fa5]", " ").Trim().ToUpper() + " ";

                // 行是否属于segment标识
                bool row_segment_sign = false;

                foreach (var v in pattern_seg_exact)
                {
                    string regexp_exact = v.Key;

                    var reg = new Regex(regexp_exact, RegexOptions.Multiline);
                    var match = reg.Match(trimLine);
                    bool is_found = match.Success;

                    if (is_found)
                    {
                        row_segment_sign = true;

                        // 确定该行是否在段名库中，如果在则判断该段是否在前面遇到过
                        if (!sectionMap.ContainsKey(pattern_seg_exact[regexp_exact]))
                        {
                            // 存在前两行“求职意向”情况,解决“求职意向”被包含在基本信息的情况
                            if (i <= 2 && currentSectionName == "basic_info"
                                    && pattern_seg_exact[regexp_exact] == "career_objective")
                            {
                                //添加“求职意向”新段（一行）
                                SectionInfo curSectionInfo = new SectionInfo(i + 1, i + 1);
                                sectionMap["career_objective"] = curSectionInfo;

                                //同时更新“基本信息”，解决“求职意向”被包含在基本信息的情况
                                sectionMap[currentSectionName].End += 1;
                            }
                            else
                            {
                                currentSectionName = pattern_seg_exact[regexp_exact];
                                SectionInfo curSectionInfo = new SectionInfo(i + 1, i + 1);
                                sectionMap[currentSectionName] = curSectionInfo;
                            }
                        }
                        else
                        {
                            // 如果遇到过，则该段名可能出现在基本信息中，就需要判断之间的段名是否是在基本信息中，
                            // 如果是在基本信息中则直接以当前段为开始，忽略之前的段开始位置
                            // 加上判断之前段是否在基本信息中的逻辑
                            sectionMap[currentSectionName].End += 1;
                        }
                        break;
                    }
                }

                if (!row_segment_sign)
                    sectionMap[currentSectionName].End += 1;
            };

            return sectionMap;
        }
    }
}