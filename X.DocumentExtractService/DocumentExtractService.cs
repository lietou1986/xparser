using Dorado;
using Dorado.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using X.DocumentExtractService.Contract;
using X.DocumentExtractService.Contract.Models;
using X.DocumentExtractService.Extractor;

namespace X.DocumentExtractService
{
    public class DocumentExtractService : IDocumentExtractService
    {
        private ExtractOption CombineOptions(ExtractOption[] options)
        {
            ExtractOption extractOption = 0;
            if (options != null && options.Length != 0)
            {
                ExtractOption[] extractOptionArray = options;
                extractOption = extractOptionArray.Aggregate(extractOption, (current, t) => current | t);
            }
            return extractOption;
        }

        public OperateResult<ExtractedResult> Extract(string path, ExtractOption[] options)
        {
            OperateResult<ExtractedResult> operateResult = new OperateResult<ExtractedResult>();
            ExtractOption extractOption = CombineOptions(options);
            string extension = Path.GetExtension(path);
            ICollection<DocumentExtractor> extractors = ExtractorFactory.GetExtractors(extension);
            if (extractors == null || extractors.Count == 0)
            {
                operateResult.Status = OperateStatus.Failure;
                operateResult.Description = "没有对应的处理程序";
                return operateResult;
            }
            bool flag = false;
            if (!File.Exists(path))
            {
                operateResult.Status = OperateStatus.Failure;
                operateResult.Description = string.Concat("不存在该文件:", path);
                return operateResult;
            }
            byte[] numArray = File.ReadAllBytes(path);
            try
            {
                foreach (DocumentExtractor extractor in extractors)
                {
                    operateResult.Data = extractor.Extract(extension, numArray, extractOption);
                    if (operateResult.Data == null)
                    {
                        continue;
                    }
                    flag = true;
                    break;
                }
            }
            catch (Exception exception)
            {
                operateResult.Status = OperateStatus.Failure;
                operateResult.Description = string.Concat("抽取出错：", exception.Message, Environment.NewLine, exception.StackTrace);
                LoggerWrapper.Logger.Error("ExtractText", exception);
            }
            if (!flag)
            {
                operateResult.Status = OperateStatus.Failure;
                operateResult.Description = "抽取出错";
            }
            return operateResult;
        }

        public OperateResult Ping()
        {
            return OperateResult.Success;
        }
    }
}