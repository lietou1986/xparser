using Dorado;
using Dorado.Extensions;
using Dorado.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using X.DocumentExtractService.Contract.Models;
using X.ResumeParseService.Contract;
using X.ResumeParseService.Scanner.Enums;
using X.ResumeParseService.Scanner.Storages;

namespace X.ResumeParseService.Scanner
{
    /// <summary>
    /// 简历文件扫描器
    /// </summary>
    public class ResumeFileScanner : FileScanner
    {
        private static string DateFormat = "yyyy-MM-dd HH:mm:ss";
        private static string Filter;
        private static int MaxSize = 3;//3M
        private static List<string> ExcludeSpecialFolderList = new List<string>();
        private static List<string> IncludeSpecialFolderList = new List<string>();
        private static string InsertFileSql = "INSERT INTO [file]([path],[md5],[timestamp],[addtime],[edittime],[guid])VALUES('{0}','{1}','{2}','{3}','{3}','{4}');";

        private bool _inited;
        private IResumeParseService _service;
        private SafeSQLite _fConn, _rConn;
        private AbsStorage _storage;
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
            ExcludeSpecialFolderList.Add(@"windows");
            ExcludeSpecialFolderList.Add(@"inetpub");
            ExcludeSpecialFolderList.Add(@"input");
            ExcludeSpecialFolderList.Add(@"users");
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
            activity.HashCode = activity.FilePath.MD5();

            RecordFileInfo(activity);

            var result = _service.Parse(activity.FilePath, new ExtractOption[] { ExtractOption.Text });
            if (result.Status == OperateStatus.Success)
            {
                _storage.Save(activity, result.Data);

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
            {
                activity.Message = "文件不存在";
                return false;
            }
            if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                activity.Message = "隐藏文件不参与扫描";
                return false;
            }

            if (Filter.IndexOf(fileInfo.Extension.ToLower()) < 0)
            {
                activity.Message = "不支持此扩展名的文件";
                return false;
            }
            if (fileInfo.Length > MaxSize * 1024 * 1024)
            {
                activity.Message = string.Format("不支持大于[{0}ｍ]的文件", MaxSize);
                return false;
            }

            //判断文件是否变化，没有变化不处理
            if (!FileChanged(activity))
            {
                activity.Message = "文件内容未发生变化";
                return false;
            }

            //检验是否为简历
            var result = _service.Predict(activity.FilePath, new ExtractOption[] { ExtractOption.Text });
            if (result.Status == OperateStatus.Failure || !result.Data.IsResume)
            {
                activity.Message = result.Description;
                return false;
            }

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

        private void RecordFileInfo(FileActivity activity)
        {
            if (activity.IsChanged)
            {
                _fConn.ExecuteScalar(string.Format("delete from file where guid='{0}'", activity.HashCode));
            }

            FileInfo fileInfo = new FileInfo(activity.FilePath);

            //记录文件信息
            _fConn.ExecuteNonQuery(string.Format(InsertFileSql, fileInfo.FullName.ToLower(), activity.Md5, fileInfo.LastWriteTime.ToString(DateFormat), DateTime.Now.ToString(DateFormat), activity.HashCode));
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
            _storage = new LocalStorage(_rConn);
            _inited = true;
        }
    }
}