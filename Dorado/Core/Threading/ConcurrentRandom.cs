using System;
using System.Runtime.InteropServices;

namespace Dorado.Core.Threading
{
    /// <summary>
    /// 用于并发环境下生成随机数
    /// </summary>
    public class ConcurrentRandom : Random
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <summary>
        /// 默认使用高性能计数器作为随机种子
        /// </summary>
        public ConcurrentRandom()
            : this(true)
        {
        }

        public ConcurrentRandom(bool perfSeed)
            : base(InitSeed(perfSeed))
        {
        }

        /// <summary>
        /// 初始化随机种子
        /// </summary>
        private static int InitSeed(bool perfSeed)
        {
            long freq;

            if (perfSeed && QueryPerformanceFrequency(out freq))
            {
                long perfCount;
                QueryPerformanceCounter(out perfCount);
                string perfCountStr = perfCount.ToString();
                return int.Parse(perfCountStr.Substring(perfCountStr.Length - 9));
            }
            //如果不支持高性能计数器用时间做随机种子
            string tickStr = DateTime.Now.Ticks.ToString() + AppDomain.GetCurrentThreadId().ToString();
            return int.Parse(tickStr.Substring(tickStr.Length - 9));
        }
    }
}