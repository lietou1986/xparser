using Dorado.Core;
using Dorado.Extensions;
using System.Collections.Generic;
using System.IO;

namespace X.ResumeParseService.Utils
{
    public class PositionTokenAnalyzer
    {
        private static Dictionary<string,int> seg_dict;

        internal static string DictPath
        {
            set
            {
                string dictPath = value;

                if (seg_dict != null && seg_dict.Count > 0)
                    return;

                seg_dict = new Dictionary<string, int>();
                string line = null;

                StreamReader br = null;
                try
                {
                    br = new StreamReader(dictPath);
                    while (!(line = br.ReadLine()).IsNullOrWhiteSpace())
                    {
                        line = line.Trim();

                        seg_dict[line]=1;
                    }
                }
                catch (IOException ex)
                {
                    LoggerWrapper.Logger.Error("CompanyTokenAnalyzer error", ex);
                }
                finally
                {
                    if (br != null)
                        br.Close();
                }
            }
        }

        /**
         * 前向算法分词
         *
         * @param seg_dict
         *            分词词典
         * @param phrase
         *            待分词句子
         * @return 前向分词结果
         */

        private static List<string> FMM2(string phrase)
        {
            int maxlen = 16;
            List<string> fmm_list = new List<string>();
            int len_phrase = phrase.Length;
            int i = 0, j = 0;

            while (i < len_phrase)
            {
                int end = i + maxlen;
                if (end >= len_phrase)
                    end = len_phrase;
                string phrase_sub = phrase.SubString(i, end);
                for (j = phrase_sub.Length; j >= 0; j--)
                {
                    if (j == 1)
                        break;
                    string key = phrase_sub.SubString(0, j);
                    if (seg_dict.ContainsKey(key))
                    {
                        fmm_list.Add(key);
                        i += key.Length - 1;
                        break;
                    }
                }
                // 排除单个字符，
                // if(j == 1)
                // fmm_list.Add(""+phrase_sub.charAt(0));
                i += 1;
            }
            return fmm_list;
        }

        /**
         * 后向算法分词
         *
         * @param seg_dict
         *            分词词典
         * @param phrase
         *            待分词句子
         * @return 后向分词结果
         */

        private static List<string> BMM2(string phrase)
        {
            int maxlen = 16;
            List<string> bmm_list = new List<string>();
            int len_phrase = phrase.Length;
            int i = len_phrase, j = 0;

            while (i > 0)
            {
                int start = i - maxlen;
                if (start < 0)
                    start = 0;
                string phrase_sub = phrase.SubString(start, i);
                for (j = 0; j < phrase_sub.Length; j++)
                {
                    if (j == phrase_sub.Length - 1)
                        break;
                    string key = phrase_sub.Substring(j);
                    if (seg_dict.ContainsKey(key))
                    {
                        bmm_list.Insert(0, key);
                        i -= key.Length - 1;
                        break;
                    }
                }
                if (j == phrase_sub.Length - 1)
                    bmm_list.Insert(0, "" + phrase_sub.ToCharArray()[j]);
                i -= 1;
            }
            return bmm_list;
        }

        /**
         * 该方法结合正向匹配和逆向匹配的结果，得到分词的最终结果
         *
         * @param FMM2
         *            正向匹配的分词结果
         * @param BMM2
         *            逆向匹配的分词结果
         * @param return
         *            分词的最终结果
         */

        public static List<string> segment(string phrase)
        {
            List<string> fmm_list = FMM2(phrase);
            List<string> bmm_list = BMM2(phrase);
            // 如果正反向分词结果词数不同，则取分词数量较少的那个
            if (fmm_list.Count != bmm_list.Count)
            {
                if (fmm_list.Count > bmm_list.Count)
                    return bmm_list;
                else
                    return fmm_list;
            }
            // 如果分词结果词数相同
            else
            {
                // 如果正反向的分词结果相同，就说明没有歧义，可返回任意一个
                int i, FSingle = 0, BSingle = 0;
                bool isSame = true;
                for (i = 0; i < fmm_list.Count; i++)
                {
                    if (fmm_list[i] != (bmm_list[i]))
                        isSame = false;
                    if (fmm_list[i].Length == 1)
                        FSingle += 1;
                    if (bmm_list[i].Length == 1)
                        BSingle += 1;
                }
                if (isSame)
                    return fmm_list;
                else
                {
                    // 分词结果不同，返回其中单字较少的那个
                    if (BSingle > FSingle)
                        return fmm_list;
                    else
                        return bmm_list;
                }
            }
        }
    }
}