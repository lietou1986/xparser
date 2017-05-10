using System;
using X.ResumeParseService.Contract;
using X.ResumeParseService.Contract.Models;

namespace X.ResumeParseService.Scanner.Storages
{
    internal class LocalStorage : AbsStorage
    {
        private static string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private static string InsertResumeSql = "INSERT INTO [ResumeList]([Guid],[UserName],[Age],[Sex],[Phone],[Email],[ResumeName],[Education],[Position],[Address],[Url],[Source],[WorkYear],[LastChangeTime],[TimeStamp],[Mac])" +
         "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}');INSERT INTO [ResumeDetailList]([Guid],[ResumeText],[TimeStamp])VALUES('{0}','{15}','{14}');";

        private SafeSQLite _conn;

        public LocalStorage(SafeSQLite conn)
        {
            _conn = conn;
        }

        protected override void _Save(FileActivity activity, ResumeResult data)
        {
            if (activity.IsChanged)
            {
                _conn.ExecuteScalar(string.Format("delete from resumelist where guid='{0}';delete from resumedetaillist where guid='{0}';", activity.HashCode));
            }

            DateTime dateTime = DateTime.Now;

            ResumeData resumeData = data.ResumeInfo;

            //记录简历信息
            _conn.ExecuteNonQuery(string.Format(InsertResumeSql, activity.HashCode, resumeData.Name, resumeData.Age, resumeData.Gender, resumeData.Phone, resumeData.Email, resumeData.JobTarget == null ? string.Empty : resumeData.JobTarget.JobCareer, resumeData.LatestDegree, resumeData.JobTarget == null ? string.Empty : resumeData.JobTarget.JobCareer, resumeData.Residence, activity.FilePath, "本地计算机", resumeData.WorkYears, dateTime.ToString(DateFormat), dateTime.ToUniversalTime(), Dorado.SystemInfo.SystemInfo.GetMacAddress(), data.Text));
        }
    }
}