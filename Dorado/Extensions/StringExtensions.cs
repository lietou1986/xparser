using System.Diagnostics;
using System.Globalization;
using System.Web.Security;

namespace Dorado.Extensions
{
    /// <summary>
    /// 字符串的工具类
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        /// Formats a string to an invariant culture
        /// </summary>
        /// <param name="format"></param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string FormatInvariant(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.InvariantCulture, format, objects);
        }

        /// <summary>
        /// Formats a string to the current culture.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="objects">The objects.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string FormatCurrent(this string format, params object[] objects)
        {
            return string.Format(CultureInfo.CurrentCulture, format, objects);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// java模式
        /// </summary>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static string SubString(this string value, int startIndex, int endIndex)
        {
            //abc
            return value.Substring(startIndex, endIndex - startIndex);
        }

        public static string SubString(this string value, int startIndex)
        {
            //abc
            return value.Substring(startIndex);
        }

        public static string MD5(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
            }
            return str;
        }
    }
}