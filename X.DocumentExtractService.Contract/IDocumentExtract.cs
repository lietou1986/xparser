using Dorado;

namespace X.DocumentExtractService.Contract
{
    /// <summary>
    /// v2.0文档抽取接口
    /// </summary>

    public interface IDocumentExtract
    {
        /// <summary>
        /// 从文件数据中提取文本内容
        /// </summary>
        /// <param name="extensionName">文件扩展名（包含句点“.”）</param>
        /// <param name="data">文件数据</param>
        /// <returns>提取的文本</returns>
        OperateResult<string> Extract(string extensionName, byte[] data);
    }
}