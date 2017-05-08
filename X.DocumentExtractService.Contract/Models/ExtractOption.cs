using System;

namespace X.DocumentExtractService.Contract.Models
{
    /// <summary>
    /// 抽取参数
    /// </summary>
    [Flags]
    public enum ExtractOption
    {
        /// <summary>
        /// 抽取文本
        /// </summary>
        Text = 1,

        /// <summary>
        /// 抽取图像
        /// </summary>
        Image = 2
    }
}