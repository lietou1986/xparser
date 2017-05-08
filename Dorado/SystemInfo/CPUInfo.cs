using System.Runtime.InteropServices;

namespace Dorado.SystemInfo
{
    /// <summary>
    /// 定义CPU的信息结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CpuInfo
    {
        /// <summary>
        /// OEM ID
        /// </summary>
        public uint dwOemId;

        /// <summary>
        /// 页面大小
        /// </summary>
        public uint dwPageSize;

        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;

        /// <summary>
        /// CPU个数
        /// </summary>
        public uint dwNumberOfProcessors;

        /// <summary>
        /// CPU类型
        /// </summary>
        public uint dwProcessorType;

        public uint dwAllocationGranularity;

        /// <summary>
        /// CPU等级
        /// </summary>
        public uint dwProcessorLevel;

        public uint dwProcessorRevision;
    }
}