using Dorado;

using X.DocumentExtractService.Contract.Models;

namespace X.ResumeParseService.Contract
{
    public interface IResumeParseService
    {
        /// <summary>
        /// 简历解析服务
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        OperateResult<ResumeResult> Parse(string path, ExtractOption[] options);

        /// <summary>
        /// 简历解析服务
        /// </summary>
        /// <returns></returns>
        OperateResult<ResumeResult> ParseText(string txt);

        /// <summary>
        /// 简历预判服务（判断是否为简历）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        OperateResult<ResumePredictResult> Predict(string path, ExtractOption[] options);

        /// <summary>
        /// 简历预判服务（判断是否为简历）
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        OperateResult<ResumePredictResult> PredictText(string text);

        /// <summary>
        /// Ping
        /// </summary>
        /// <returns></returns>
        OperateResult Ping();
    }
}