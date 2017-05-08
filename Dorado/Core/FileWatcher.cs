using Dorado.Core.Threading;
using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Dorado.Core
{
    /// <summary>
    /// 目录监控，检测文件有无变化（通用类）
    /// </summary>
    internal sealed class DirectoryWatcher
    {
        private RwLocker filesLock;
        private Dictionary<string, EventHandler> files;
        private List<string> pendingFileReloads;
        private int changeFileDelay;
        private string directory;
        private FileSystemWatcher scareCrow;
        private string filter;

        public DirectoryWatcher(string directory, int changeDelay, string filter)
        {
            filesLock = new RwLocker();
            files = new Dictionary<string, EventHandler>();
            this.directory = directory;
            this.filter = filter;
            pendingFileReloads = new List<string>();
            changeFileDelay = changeDelay;
            InitWatcher();
        }

        /// <summary>
        /// 只在 DirectoryWatcher创建时执行一次
        /// </summary>
        /// <param name="directory"></param>
        private void InitWatcher()
        {
            scareCrow = new FileSystemWatcher();
            scareCrow.Path = directory;
            if (!filter.IsNullOrWhiteSpace())
                scareCrow.Filter = filter;
            scareCrow.Changed += scareCrow_Changed;
            scareCrow.EnableRaisingEvents = true;
            scareCrow.IncludeSubdirectories = true;
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="delegateMethod">委托(string filePath,Empty), 文件地址用小写</param>
        internal void AddFile(string fileName, EventHandler delegateMethod)
        {
            fileName = fileName.ToLower();

            using (filesLock.Write())
            {
                if (!files.ContainsKey(fileName))
                    files.Add(fileName, delegateMethod);
            }
        }

        private EventHandler GetEventHandler(string fileName)
        {
            fileName = fileName.ToLower();
            EventHandler handler;

            using (filesLock.Read())
            {
                files.TryGetValue(fileName, out handler);
            }
            return handler;
        }

        private bool ContainsFile(string fileName)
        {
            fileName = fileName.ToLower();
            bool contains;

            using (filesLock.Read())
            {
                contains = files.ContainsKey(fileName);
            }
            return contains;
        }

        private void scareCrow_Changed(object sender, FileSystemEventArgs e)
        {
            string fileName = e.Name.ToLower();

            if (File.GetAttributes(e.FullPath) == FileAttributes.Directory)
            {
                return;
            }

            using (filesLock.Upgrade())
            {
                if (pendingFileReloads.Contains(fileName) || !ContainsFile(fileName))
                    return;

                pendingFileReloads.Add(fileName);
            }
            CountdownTimer timer = new CountdownTimer();
            timer.BeginCountdown(changeFileDelay, DelayedProcessFileChanged, fileName);
        }

        public void ProcessFileChanged(string fileName)
        {
            EventHandler delegateMethod = GetEventHandler(fileName);
            if (delegateMethod != null)
            {
                try
                {
                    string filePath = directory + "\\" + fileName;
                    delegateMethod(filePath, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    LoggerWrapper.Logger.Error("FileWatcher", ex);
                }
            }
        }

        private void DelayedProcessFileChanged(IAsyncResult ar)
        {
            string fileName = (string)ar.AsyncState;

            using (filesLock.Write())
            {
                pendingFileReloads.Remove(fileName);
            }

            //文件与处理器一对一!!
            ProcessFileChanged(fileName);
        }
    }

    public class FileWatcher
    {
        private object dirsLock = new object();
        private Dictionary<string, DirectoryWatcher> directories;
        private static FileWatcher instance = new FileWatcher();
        public string Filter { get; set; }

        public static FileWatcher Instance
        {
            get
            {
                return instance;
            }
        }

        public FileWatcher(string filter = "")
        {
            if (!filter.IsNullOrWhiteSpace())
                Filter = filter.ToLower();

            dirsLock = new object();
            directories = new Dictionary<string, DirectoryWatcher>();
        }

        public void AddFile(string filePath, EventHandler handler, int changeFileDelay = 5000, bool checkFile = false)
        {
            if (checkFile)
            {
                Guard.ArgumentIsFile(filePath);
            }
            FileInfo fileInfo = new FileInfo(filePath);
            if (!Filter.IsNullOrWhiteSpace())
            {
                string ext = fileInfo.Extension.ToLower();
                if (Filter.IndexOf(ext) < 0)
                    throw new CoreException("监控的文件格式无效，因为文件监听过滤器中不包含这类文件类型，监听无效，文件路径:{0}", filePath);
            }

            string dir = fileInfo.DirectoryName;
            string fileName = fileInfo.Name;

            DirectoryWatcher watcher;
            lock (dirsLock)
            {
                if (!directories.TryGetValue(dir, out watcher))
                {
                    watcher = new DirectoryWatcher(dir, changeFileDelay, Filter);
                    directories.Add(dir, watcher);
                }
            }
            watcher.AddFile(fileName, handler);
        }

        public void ProcessFileChanged(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath).ToLower();
            string fileName = Path.GetFileName(filePath).ToLower();
            DirectoryWatcher watcher;
            lock (dirsLock)
            {
                if (!directories.TryGetValue(dir, out watcher))
                    return;
            }
            watcher.ProcessFileChanged(fileName);
        }
    }
}