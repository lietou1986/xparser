using System.Runtime.InteropServices;

namespace Dorado.SystemInfo
{
    //// <summary>
    /// 定义系统时间的信息结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTimeInfo
    {
        /// <summary>
        /// 年
        /// </summary>
        public ushort wYear;

        /// <summary>
        /// 月
        /// </summary>
        public ushort wMonth;

        /// <summary>
        /// 星期
        /// </summary>
        public ushort wDayOfWeek;

        /// <summary>
        /// 天
        /// </summary>
        public ushort wDay;

        /// <summary>
        /// 小时
        /// </summary>
        public ushort wHour;

        /// <summary>
        /// 分钟
        /// </summary>
        public ushort wMinute;

        /// <summary>
        /// 秒
        /// </summary>
        public ushort wSecond;

        /// <summary>
        /// 毫秒
        /// </summary>
        public ushort wMilliseconds;
    }
}