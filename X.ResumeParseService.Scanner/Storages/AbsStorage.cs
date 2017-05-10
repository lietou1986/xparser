using Dorado.Core;
using System;
using X.ResumeParseService.Contract;

namespace X.ResumeParseService.Scanner.Storages
{
    internal abstract class AbsStorage
    {
        public bool Save(FileActivity activity, ResumeResult data)
        {
            try
            {
                _Save(activity, data);
                return true;
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("简历数据存储异常", ex);
                return false;
            }
        }

        protected abstract void _Save(FileActivity activity, ResumeResult data);
    }
}