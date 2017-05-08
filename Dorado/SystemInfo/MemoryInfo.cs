using System.Runtime.InteropServices;

namespace Dorado.SystemInfo
{
    //// <summary>
    /// 定义内存的信息结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryInfo
    {
        /// <summary>
        ///
        /// </summary>
        public uint dwLength;

        /// <summary>
        /// 已经使用的内存
        /// </summary>
        public uint dwMemoryLoad;

        /// <summary>
        /// 总物理内存大小
        /// </summary>
        public uint dwTotalPhys;

        /// <summary>
        /// 可用物理内存大小
        /// </summary>
        public uint dwAvailPhys;

        /// <summary>
        /// 交换文件总大小
        /// </summary>
        public uint dwTotalPageFile;

        /// <summary>
        /// 可用交换文件大小
        /// </summary>
        public uint dwAvailPageFile;

        /// <summary>
        /// 总虚拟内存大小
        /// </summary>
        public uint dwTotalVirtual;

        /// <summary>
        /// 可用虚拟内存大小
        /// </summary>
        public uint dwAvailVirtual;
    }
}