using Dorado;
using Dorado.Core;
using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using X.ResumeParseService.Contract;

namespace X.ResumeParseService.Scanner
{
    /// <summary>
    /// 简历文件扫描器
    /// </summary>
    public class ResumeParser
    {
        private FileScanner _fileScanner;
        private List<string> _directorys;

        public ResumeParser()
        {
            _directorys = new List<string>();
            List<DriveInfo> drives = Directory.GetLogicalDrives().Select(q => new DriveInfo(q)).ToList();
            foreach (var driveInfo in drives)
            {
                if (driveInfo.IsReady && driveInfo.DriveType == DriveType.Fixed)
                {
                    _directorys.Add(driveInfo.Name);
                }
            }
        }

        public void Start()
        {
#if DEBUG

            Console.Clear();
#endif

            if (_directorys.Count == 0)
            {
                LoggerWrapper.Logger.Error("无有效磁盘驱动器");
                return;
            }

            if (_fileScanner == null || _fileScanner.Completed)
            {
                _fileScanner = new ResumeFileScanner(_directorys, true, 1, FileEnqueueCallBack, FileHandleCallBack, ScanOverCallBack, HandleOverCallBack);
                LoggerWrapper.Logger.Info("扫描器初始化完成,启动中...");
            }

            _fileScanner.Start();

            LoggerWrapper.Logger.Info("简历解析开始...");
        }

        public void Stop()
        {
            if (_fileScanner != null)
            {
                _fileScanner.Stop();
            }
        }

        #region 扫描器回调

        private void FileEnqueueCallBack(FileActivity activity)
        {
#if DEBUG
            Console.WriteLine("扫描文件->" + activity.FilePath);

            if (activity.IsChanged)
                Console.WriteLine(string.Format("文件发生变化->{0}", activity.FilePath));
            if (!activity.IsValid)
                Console.WriteLine(string.Format("文件无效->{0}", activity.FilePath));
#endif
        }

        private void FileHandleCallBack(FileActivity activity, OperateResult<ResumeResult> result)
        {
#if DEBUG
            Console.WriteLine("处理文件->" + activity.FilePath);
#endif
        }

        private void ScanOverCallBack()
        {
            string message = string.Format("扫描完成！ 共扫描文件[{0}]，有效入队文件[{1}]，耗时[{2}s]", _fileScanner.FileCount, _fileScanner.EnqueueCount, _fileScanner.TotalTime.ToString("f2"));
#if DEBUG
            Console.WriteLine(message);
#endif
            LoggerWrapper.Logger.Info(message);
        }

        private void HandleOverCallBack()
        {
            string message = string.Format("处理完成！ 共扫描文件[{0}]，有效入队文件[{1}]，处理文件[{2}]，耗时[{3}s]", _fileScanner.FileCount, _fileScanner.EnqueueCount, _fileScanner.HandleCount, _fileScanner.TotalTime.ToString("f2"));
#if DEBUG
            Console.WriteLine(message);
            Console.WriteLine("等待下次处理....");
#endif
            LoggerWrapper.Logger.Info(message);
            LoggerWrapper.Logger.Info("等待下次处理....");

            string scanInterval = ConfigurationManager.AppSettings["ScanInterval"];
            if (scanInterval.IsNullOrWhiteSpace())
                Thread.Sleep(1000 * 60 * 30);
            else
                Thread.Sleep(1000 * 60 * int.Parse(scanInterval));

            Start();
        }

        #endregion 扫描器回调
    }
}