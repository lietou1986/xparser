using Dorado;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Contract
{
    /// <summary>
    /// v3.0文档抽取接口,支持头像的抽取
    /// </summary>

    public interface IDocumentExtractService
    {
        /// <summary>
        /// 从文本数据中抽取内容 {"Options":[1],"Path":"文件地址"}
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        OperateResult<ExtractedResult> Extract(string path, ExtractOption[] options);

        /// <summary>
        /// Ping
        /// </summary>
        /// <returns></returns>
        OperateResult Ping();
    }
}