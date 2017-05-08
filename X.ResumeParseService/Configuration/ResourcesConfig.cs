using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using X.ResumeParseService.Utils;

namespace X.ResumeParseService.Configuration
{
    /// <summary>
    ///  加载所有资源，用于建立解析
    /// </summary>
    public class ResourcesConfig
    {
        public static HashSet<string> segmentTitleSet = new HashSet<string>();
        public static HashSet<string> lastNameSet = new HashSet<string>();
        public static HashSet<string> fadeNameSet = new HashSet<string>();
        public static List<string> lastNameArray = new List<string>();
        public static HashSet<string> universitySet = new HashSet<string>();
        public static HashSet<string> universityEndsSet = new HashSet<string>();
        public static HashSet<string> roleSet = new HashSet<string>();
        public static HashSet<string> degreeSet = new HashSet<string>();
        public static HashSet<string> rolesKeywordsSet = new HashSet<string>();
        public static List<string> majorList = new List<string>();

        public static string nameStartRegex = "";
        public static string namefadeRegex = "";
        public static string degreeRegex = "";
        public static string universityRegex = "";
        public static string rolesKeywordsRegex = "";
        public static string majorRegex = "";

        private static bool inited = false;

        public static void Load()
        {
            if (inited) return;

            string folder = AppDomain.CurrentDomain.BaseDirectory + "Resources\\";

            PositionTokenAnalyzer.DictPath = folder + "positions_final.txt";
            CompanyTokenAnalyzer.DictPath = folder + "comp_non_postfix.txt";

            string segmentTitlefile = folder + "SegmentName.txt";
            string lastNamefile = folder + "LastNames.txt";
            string fadeNamefile = folder + "fadeNames.txt";

            string universityFile = folder + "Universities.txt";
            string universityEndsFile = folder + "UniversityEnds.txt";
            string rolesFile = folder + "Roles.txt";
            string degreeFile = folder + "Degrees.txt";
            string rolesKeywordsFile = folder + "Roles.txt";
            string majorFile = folder + "majors.txt";

            StreamReader segmentTitleReader = null;
            StreamReader lastNameReader = null;
            StreamReader fadeNameReader = null;

            StreamReader universityReader = null;
            StreamReader universityEndsReader = null;
            StreamReader rolesReader = null;
            StreamReader degreeReader = null;
            StreamReader rolesKeywordsReader = null;
            StreamReader majorReader = null;

            try
            {
                segmentTitleReader = new StreamReader(segmentTitlefile);
                lastNameReader = new StreamReader(lastNamefile);
                fadeNameReader = new StreamReader(fadeNamefile);

                universityReader = new StreamReader(universityFile);
                universityEndsReader = new StreamReader(universityEndsFile);
                rolesReader = new StreamReader(rolesFile);
                degreeReader = new StreamReader(degreeFile);
                rolesKeywordsReader = new StreamReader(rolesKeywordsFile);
                majorReader = new StreamReader(majorFile);

                string line = "";

                // 一次读入一行，直到读入null为文件结束
                while (!(line = segmentTitleReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    segmentTitleSet.Add(line);
                }

                while (!(line = lastNameReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    lastNameSet.Add(line);
                    lastNameArray.Add(line);
                }

                while (!(line = fadeNameReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    fadeNameSet.Add(line);
                }

                while (!(line = universityReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    universitySet.Add(line);
                }

                while (!(line = universityEndsReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    universityEndsSet.Add(line);
                }

                while (!(line = rolesReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    roleSet.Add(line);
                }

                while (!(line = degreeReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    degreeSet.Add(line);
                }

                while (!(line = rolesKeywordsReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    rolesKeywordsSet.Add(line);
                }

                while (!(line = majorReader.ReadLine()).IsNullOrWhiteSpace())
                {
                    line = line.Trim();
                    majorList.Add(line);
                }

                lastNameArray.ForEach(n =>
                {
                    if (nameStartRegex == "")
                    {
                        nameStartRegex = n;
                    }
                    else
                    {
                        nameStartRegex = nameStartRegex + "|" + n;
                    }
                });

                // major过滤
                majorList.Sort();

                majorList.ForEach(n =>
                {
                    if (majorRegex == "")
                    {
                        majorRegex = n;
                    }
                    else
                    {
                        majorRegex = majorRegex + "|" + n;
                    }
                });

                // 错写名过滤
                foreach (var n in fadeNameSet)
                {
                    if (namefadeRegex == "")
                    {
                        namefadeRegex = n;
                    }
                    else
                    {
                        namefadeRegex = namefadeRegex + "|" + n;
                    }
                }

                foreach (var n in degreeSet)
                {
                    if (degreeRegex == "")
                    {
                        degreeRegex = n;
                    }
                    else
                    {
                        degreeRegex = degreeRegex + "|" + n;
                    }
                }

                foreach (var n in universitySet)

                {
                    if (universityRegex == "")
                    {
                        universityRegex = n;
                    }
                    else
                    {
                        universityRegex = universityRegex + "|" + n;
                    }
                }

                foreach (var n in rolesKeywordsSet)
                {
                    if (rolesKeywordsRegex == "")
                    {
                        rolesKeywordsRegex = n;
                    }
                    else
                    {
                        rolesKeywordsRegex = rolesKeywordsRegex + "|" + n;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                segmentTitleReader.Close();
                lastNameReader.Close();
                universityReader.Close();
                universityEndsReader.Close();
                rolesReader.Close();
                degreeReader.Close();
                inited = true;
            }
        }

        public static void Reload()
        {
            inited = false;
            Load();
        }
    }
}