using Dorado;
using Dorado.Core;
using Dorado.Extensions;
using System;
using System.Collections.Generic;
using X.DocumentExtractService.Contract;
using X.DocumentExtractService.Contract.Models;
using X.DocumentExtractService.Extractor;

namespace X.DocumentExtractService
{
    public class ExtractService : IDocumentExtract
    {
        public OperateResult<string> Extract(string extensionName, byte[] data)
        {
            OperateResult<string> operateResult = new OperateResult<string>();
            try
            {
                if (extensionName.IsNullOrWhiteSpace())
                {
                    operateResult.Status = OperateStatus.Failure;
                    operateResult.Description = "extension不能为空";
                }
                else if (data == null || data.Length == 0)
                {
                    operateResult.Status = OperateStatus.Failure;
                    operateResult.Description = "fileData不能为空";
                }
                else
                {
                    ICollection<DocumentExtractor> extractors = ExtractorFactory.GetExtractors(extensionName);
                    if (extractors == null || extractors.Count == 0)
                    {
                        operateResult.Status = OperateStatus.Failure;
                        operateResult.Description = "没有对应的处理程序";
                    }
                    else
                    {
                        bool flag = false;
                        foreach (DocumentExtractor extractor in extractors)
                        {
                            try
                            {
                                ExtractedResult extractedResult = extractor.Extract(extensionName, data, ExtractOption.Text);
                                if (extractedResult != null && extractedResult.Text != null)
                                {
                                    operateResult.Data = extractedResult.Text;
                                    flag = true;
                                    break;
                                }
                            }
                            catch (Exception exception)
                            {
                                flag = false;
                                LoggerWrapper.Logger.Warn("抽取时发生错误", exception);
                            }
                        }
                        if (!flag)
                        {
                            operateResult.Status = OperateStatus.Failure;
                            operateResult.Description = "抽取出错";
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                operateResult.Description = "抽取出错";
                LoggerWrapper.Logger.Error("ExtractText", exception);
            }
            return operateResult;
        }
    }
}