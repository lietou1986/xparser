using System;
using X.ResumeParseService.Contract;

namespace X.ResumeParseService.Scanner.Storages
{
    internal class RemoteStorage : AbsStorage
    {
        protected override void _Save(FileActivity activity, ResumeResult data)
        {
            throw new NotImplementedException();
        }
    }
}