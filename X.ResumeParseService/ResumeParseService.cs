using Dorado;
using Dorado.Core;
using Dorado.Extensions;
using System;
using X.DocumentExtractService.Contract;
using X.DocumentExtractService.Contract.Models;
using X.ResumeParseService.Contract;
using X.ResumeParseService.Contract.Models;

namespace X.ResumeParseService
{
    public class ResumeParseService : IResumeParseService
    {
        public OperateResult<ResumeResult> Parse(string path, ExtractOption[] options)
        {
            try
            {
                //先判别文件是否是有效简历
                OperateResult<ResumePredictResult> predictResult = Predict(path, options);

                if (predictResult.Status == OperateStatus.Failure)
                    return new OperateResult<ResumeResult>(OperateStatus.Failure, predictResult.Description);

                ResumeResult resumeResult = new ResumeResult
                {
                    Text = predictResult.Data.Text,
                    IsResume = predictResult.Data.IsResume,
                    Score = predictResult.Data.Score
                };

                if (!resumeResult.IsResume)
                    return new OperateResult<ResumeResult>(OperateStatus.Failure, string.Format("不是有效的简历格式,得分[{0}]", predictResult.Data.Score), resumeResult);

                //文本简历解析
                TextCVParser parser = new TextCVParser(predictResult.Data.Text);
                ResumeData resumeData = parser.Parse();
                resumeResult.ResumeInfo = resumeData;
                return new OperateResult<ResumeResult>(OperateStatus.Success, "操作成功", resumeResult);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("简历解析错误", ex);
                return new OperateResult<ResumeResult>(OperateStatus.Failure, ex.Message);
            }
        }

        public OperateResult<ResumeResult> ParseText(string text)
        {
            try
            {
                //先判别文件是否是有效简历
                OperateResult<ResumePredictResult> predictResult = PredictText(text);

                if (predictResult.Status == OperateStatus.Failure)
                    return new OperateResult<ResumeResult>(OperateStatus.Failure, predictResult.Description);

                ResumeResult resumeResult = new ResumeResult
                {
                    Text = predictResult.Data.Text,
                    IsResume = predictResult.Data.IsResume,
                    Score = predictResult.Data.Score
                };

                if (!resumeResult.IsResume)
                    return new OperateResult<ResumeResult>(OperateStatus.Failure, string.Format("不是有效的简历格式,得分[{0}]", predictResult.Data.Score), resumeResult);

                //文本简历解析
                TextCVParser parser = new TextCVParser(predictResult.Data.Text);
                ResumeData resumeData = parser.Parse();
                resumeResult.ResumeInfo = resumeData;
                return new OperateResult<ResumeResult>(OperateStatus.Success, "操作成功", resumeResult);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("简历解析错误", ex);
                return new OperateResult<ResumeResult>(OperateStatus.Failure, ex.Message);
            }
        }

        public OperateResult<ResumePredictResult> Predict(string path, ExtractOption[] options)
        {
            OperateResult<ResumePredictResult> operateResult = new OperateResult<ResumePredictResult>();
            IDocumentExtractService documentExtractService = new DocumentExtractService.DocumentExtractService();
            var extractResult = documentExtractService.Extract(path, options);
            if (extractResult.Status == OperateStatus.Success)
            {
                var resumePredictResult = new ResumePredictResult
                {
                    Text = extractResult.Data.Text,
                    Images = extractResult.Data.Images,
                    Score = ResumeChecker.Predict(extractResult.Data.Text)
                };
                resumePredictResult.IsResume = resumePredictResult.Score >= 60;
                operateResult.Data = resumePredictResult;
            }
            else
            {
                operateResult.Status = extractResult.Status;
                operateResult.Description = extractResult.Description;
            }
            return operateResult;
        }

        public OperateResult<ResumePredictResult> PredictText(string text)
        {
            try
            {
                if (text.IsNullOrWhiteSpace())
                    throw new CoreException("简历内容不能为空");

                var resumePredictResult = new ResumePredictResult
                {
                    Text = text,
                    Score = ResumeChecker.Predict(text)
                };
                resumePredictResult.IsResume = resumePredictResult.Score >= 60;

                return new OperateResult<ResumePredictResult>(OperateStatus.Success, "操作成功", resumePredictResult);
            }
            catch (Exception ex)
            {
                return new OperateResult<ResumePredictResult>(OperateStatus.Failure, ex.Message);
            }
        }

        public OperateResult Ping()
        {
            return OperateResult.Success;
        }
    }
}