using System;
using System.Text.RegularExpressions;

namespace X.ResumeParseService.Utils
{
    /***
     * 日期类工具
     *
     * @author haiming.yin
     *
     */

    public class DateTools
    {
        public static string dateFormat(string dateStr)
        {
            string dateTemp = "";
            dateStr = dateStr.Replace(" ", "");
            if (dateStr.Contains("今") || dateStr.Contains("现在"))
            {
                dateTemp = "至今";
                return dateTemp;
            }

            string[] date_end_items = Regex.Split(dateStr.Trim(), "\\D");
            foreach (string item in date_end_items)
                if (item.Trim() != "")
                {
                    if (dateTemp == "")
                        dateTemp = item;
                    else
                        dateTemp += ("-" + item);
                }

            string[] items = dateTemp.Split(new char[] { '-' });
            int workStartYear = Convert.ToInt32(items[0]);
            if (workStartYear < 30)
                workStartYear += 2000;
            else if (workStartYear < 1900)
                workStartYear += 1900;

            string dateFinal = workStartYear.ToString();

            for (int i = 1; i < items.Length; i++)
                dateFinal += ("-" + items[i]);

            return dateFinal;
        }
    }
}