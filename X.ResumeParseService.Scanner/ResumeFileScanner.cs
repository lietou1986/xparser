using Dorado;
using Dorado.Extensions;
using Dorado.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using X.DocumentExtractService.Contract.Models;
using X.ResumeParseService.Scanner.Enums;
using X.ResumeParseService.Contract;
using X.ResumeParseService.Contract.Models;

namespace X.ResumeParseService.Scanner
{
    /// <summary>
    /// 简历文件扫描器
    /// </summary>
    public class ResumeFileScanner : FileScanner
    {
        private static string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private static string Filter;
        private static int MaxLength = 5;//5M
        private static List<string> ExcludeSpecialFolderList = new List<string>();
        private static List<string> IncludeSpecialFolderList = new List<string>();
        private static string InsertFileSql = "INSERT INTO [file]([path],[md5],[timestamp],[addtime],[edittime],[guid])VALUES('{0}','{1}','{2}','{3}','{3}','{4}');";

        private static string InsertResumeSql = "INSERT INTO [ResumeList]([Guid],[UserName],[Age],[Sex],[Phone],[Email],[ResumeName],[Education],[Position],[Address],[Url],[Source],[WorkYear],[LastChangeTime],[TimeStamp],[Mac])" +
            "VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}');INSERT INTO [ResumeDetailList]([Guid],[ResumeText],[TimeStamp])VALUES('{0}','{15}','{14}');";

        private bool _inited;
        private IResumeParseService _service;
        private SafeSQLite _fConn, _rConn;

        private Action<FileActivity, OperateResult<ResumeResult>> _handleCallback;
        private Action<FileActivity> _enqueueCallback;
        private Action _handleOverCallback, _scanOverCallback;

        static ResumeFileScanner()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1024;

            Filter = "*.doc;*.txt;*.docx;*.pdf;*.html;*.htm;*.rtf;*.mht";
            //排除系统目录
            Type excludeSpecialFolder = typeof(ExcludeSpecialFolder);
            foreach (var v in Enum.GetNames(excludeSpecialFolder))
            {
                try
                {
                    string value = Enum.Format(excludeSpecialFolder, Enum.Parse(excludeSpecialFolder, v), "d");
                    string folderPath = Environment.GetFolderPath((Environment.SpecialFolder)int.Parse(value));
                    if (folderPath.IsNullOrWhiteSpace())
                        continue;
                    ExcludeSpecialFolderList.Add(folderPath.ToLower().Substring(3));
                }
                catch (Exception ex)
                {
                }
            }
            ExcludeSpecialFolderList.Add(@"inetpub\");
            ExcludeSpecialFolderList.Add(@"input\");
            ExcludeSpecialFolderList.Add(@"users\");
            ExcludeSpecialFolderList.Add(@"program files");

            //包含的系统目录
            Type includeSpecialFolder = typeof(IncludeSpecialFolder);
            foreach (var v in Enum.GetNames(includeSpecialFolder))
            {
                try
                {
                    string value = Enum.Format(includeSpecialFolder, Enum.Parse(includeSpecialFolder, v), "d");
                    string folderPath = Environment.GetFolderPath((Environment.SpecialFolder)int.Parse(value));
                    if (folderPath.IsNullOrWhiteSpace())
                        continue;
                    IncludeSpecialFolderList.Add(folderPath.ToLower().Substring(3));
                }
                catch (Exception ex)
                {
                }
            }
        }

        public ResumeFileScanner(IList<string> directory, bool async = true, int theadCount = 1, Action<FileActivity> enqueueCallback = null, Action<FileActivity, OperateResult<ResumeResult>> handleCallback = null, Action scanOverCallback = null, Action handleOverCallback = null) : base("resume", directory, async, theadCount, true)
        {
            _handleCallback = handleCallback;
            _enqueueCallback = enqueueCallback;
            _scanOverCallback = scanOverCallback;
            _handleOverCallback = handleOverCallback;
            if (!_inited)
                Init();
        }

        /// <summary>
        /// 处理事件（核心功能，解析文件存储，监听文件，回调）
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected override bool HandleActivity(FileActivity activity)
        {
            var result = _service.Parse(activity.FilePath, new ExtractOption[] { ExtractOption.Text });
            if (result.Status == OperateStatus.Success)
            {
                RecordInfo(activity, result.Data);

                WatchFile(activity);
            }

            _handleCallback(activity, result);

            return true;
        }

        protected override void EnqueueActivity(FileActivity activity)
        {
            _enqueueCallback(activity);
        }

        protected override bool VerifyFile(FileActivity activity)
        {
            FileInfo fileInfo = new FileInfo(activity.FilePath);
            if (!fileInfo.Exists)
                return false;
            if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                return false;

            if (Filter.IndexOf(fileInfo.Extension.ToLower()) < 0)
                return false;
            if (fileInfo.Length > MaxLength * 1024 * 1024)
                return false;

            //判断文件是否变化，没有变化不处理
            if (!FileChanged(activity))
                return false;

            //检验是否为简历
            var result = _service.Predict(activity.FilePath, new ExtractOption[] { ExtractOption.Text });
            if (result.Status == OperateStatus.Failure || !result.Data.IsResume)
                return false;

            return true;
        }

        protected override bool VerifyDirectory(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists)
                return false;

            if (directoryInfo.FullName != directoryInfo.Root.FullName && (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                return false;

            string fullName = directoryInfo.FullName.ToLower().Substring(3);

            if (IncludeSpecialFolderList.Any(n => fullName.StartsWith(n)))
                return true;

            if (ExcludeSpecialFolderList.Any(n => fullName.StartsWith(n)))
                return false;

            return true;
        }

        protected override void HandleOverCallBack()
        {
            _handleOverCallback();

            if (_fConn != null)
                _fConn.CloseConnection();

            if (_rConn != null)
                _rConn.CloseConnection();
        }

        protected override void ScanOverCallBack()
        {
            _scanOverCallback();
        }

        /// <summary>
        ///数据库写入(data/data.sqllite)
        /// </summary>
        private void RecordInfo(FileActivity activity, ResumeResult resume)
        {
            ResumeData resumeData = resume.ResumeInfo;

            string guid = string.Empty;

            if (activity.IsChanged)
            {
                var result = _fConn.ExecuteScalar(string.Format("select guid from file nolock where path='{0}'", activity.FilePath));
                if (result != null)
                {
                    guid = result.ToString();
                    _fConn.ExecuteScalar(string.Format("delete from file where guid='{0}'", guid));
                    _rConn.ExecuteScalar(string.Format("delete from resumelist where guid='{0}';delete from resumedetaillist where guid='{0}';", guid));
                }
            }
            if (guid.IsNullOrWhiteSpace())
            {
                guid = Guid.NewGuid().ToString();
            }

            DateTime dateTime = DateTime.Now;
            FileInfo fileInfo = new FileInfo(activity.FilePath);

            //记录文件信息
            _fConn.ExecuteNonQuery(string.Format(InsertFileSql, fileInfo.FullName.ToLower(), activity.Md5, fileInfo.LastWriteTime.ToString(DateFormat), dateTime.ToString(DateFormat), guid));
            //记录简历信息
            _rConn.ExecuteNonQuery(string.Format(InsertResumeSql, guid, resumeData.Name, resumeData.Age, resumeData.Gender, resumeData.Phone, resumeData.Email, resumeData.JobTarget == null ? string.Empty : resumeData.JobTarget.JobCareer, resumeData.LatestDegree, resumeData.JobTarget == null ? string.Empty : resumeData.JobTarget.JobCareer, resumeData.Residence, activity.FilePath, "本地计算机", resumeData.WorkYears, dateTime.ToString(DateFormat), dateTime.ToUniversalTime(), Dorado.SystemInfo.SystemInfo.GetMacAddress(), resume.Text));
        }

        /// <summary>
        /// 校验文件md5变化(data/data.sqllite)
        /// </summary>
        private bool FileChanged(FileActivity activity)
        {
            activity.Md5 = IOUtility.GetFileMD5(activity.FilePath).ToLower();

            var value = _fConn.ExecuteScalar(string.Format("select md5 from file nolock where path='{0}'", activity.FilePath));
            if (value == null)
                return true;

            var result = value.ToString() != activity.Md5;
            if (result)
                activity.IsChanged = true;
            return result;
        }

        /// <summary>
        /// 系统初始化
        /// </summary>
        private void Init()
        {
            DirectoryInfo baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            _service = new ResumeParseService();
            _fConn = new SafeSQLite(string.Format("Data Source={0}", AppDomain.CurrentDomain.BaseDirectory + "data\\fdata.db"), true);
            _rConn = new SafeSQLite(string.Format("Data Source={0}", baseDirectory.Parent.FullName + "\\data\\rdata.db"), true);
            _inited = true;
        }
    }
}