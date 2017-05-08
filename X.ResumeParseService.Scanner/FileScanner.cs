using Dorado.Core;
using Dorado.Queue.Persistence;
using Dorado.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace X.ResumeParseService.Scanner
{
    /// <summary>
    /// 持久化文件扫描器
    /// </summary>
    public abstract class FileScanner
    {
        private ManualResetEvent _enqueueResetEvent, _asyncResetEvent; //信号量
        private int _enqueueCount, _handleCount, _fileCount;
        private bool _async;//同步异步（边入队边处理还是入队完成再处理）
        private FileWatcher _watcher;
        private DateTime _startTime, _endTime;
        private PersistentQueueProcessor<FileActivity> _queue;

        /// <summary>
        /// 是否运行中
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// 是否扫描完成
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public double TotalTime
        {
            get
            {
                if (_startTime == DateTime.MinValue) return 0;
                if (_endTime == DateTime.MinValue)
                    return (DateTime.Now - _startTime).TotalSeconds;
                else
                    return (_endTime - _startTime).TotalSeconds;
            }
        }

        /// <summary>
        /// 是否扫描完成
        /// </summary>
        public bool ScanCompleted { get; private set; }

        /// <summary>
        /// 队列长度
        /// </summary>
        public int QueueLength { get => _queue.Length; }

        /// <summary>
        /// 入队数量
        /// </summary>
        public int EnqueueCount { get => _enqueueCount; }

        /// <summary>
        /// 处理数量
        /// </summary>
        public int HandleCount { get => _handleCount; }

        /// <summary>
        /// 文件总数量
        /// </summary>
        public int FileCount { get => _fileCount; }

        /// <summary>
        /// 文件扫描器
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="rootDirectory">扫描的目录集合</param>
        /// <param name="async">同步异步</param>
        public FileScanner(string queueName, IList<string> directory, bool async, int theadCount, bool queueReset)
        {
            if (theadCount < 1 || theadCount > 1000)
                theadCount = Environment.ProcessorCount;

            _async = async;
            _watcher = new FileWatcher();
            _enqueueResetEvent = new ManualResetEvent(false);
            _asyncResetEvent = new ManualResetEvent(_async);
            _queue = new PersistentQueueProcessor<FileActivity>(queueName, TryHandleActivity, theadCount, ThreadPriority.Normal);
            if (queueReset)
                _queue.Purge();

            ThreadPool.QueueUserWorkItem((w) =>
              {
                  try
                  {
                      IOUtility.Traversing(directory, n =>
                      {
                          //等待信号入队
                          _enqueueResetEvent.WaitOne();
                          //入队
                          TryEnqueueActivity(new FileActivity(n.FullName.ToLower()));
                      }, null, TryVerifyDirectory, n => { LoggerWrapper.Logger.Error("FileScanner Error", n); });
                  }
                  catch (Exception ex)
                  {
                      LoggerWrapper.Logger.Error("FileScanner Traversing Error", ex);
                  }
                  finally
                  {
                      //扫描完成回调
                      TryScanOverCallBack();
                  }
              });
        }

        /// <summary>
        /// 开始处理
        /// </summary>
        public void Start()
        {
            if (Running) return;

            if (_startTime == DateTime.MinValue)
                _startTime = DateTime.Now;

            Running = true;

            _enqueueResetEvent.Set();
            _asyncResetEvent.WaitOne();
            _queue.Start();
        }

        /// <summary>
        /// 结束处理
        /// </summary>
        public void Stop()
        {
            Running = false;
            _enqueueResetEvent.Reset();
            _queue.Stop();
        }

        /// <summary>
        /// 事件处理器
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        private bool TryHandleActivity(PersistentQueueProcessor<FileActivity> queue, FileActivity activity)
        {
            bool result;
            try
            {
                result = HandleActivity(activity);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("FileScanner Handle", ex);
                result = true;
            }
            finally
            {
                //文件处理是多线程，需要原子操作
                Interlocked.Increment(ref _handleCount);
                if (ScanCompleted)
                {
                    if (_enqueueCount == _handleCount)
                    {
                        TryHandleOverCallBack();
                    }
                }
            }
            return result;
        }

        protected abstract bool HandleActivity(FileActivity activity);

        /// <summary>
        /// 入队处理器
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="activity"></param>
        private void TryEnqueueActivity(FileActivity activity)
        {
            try
            {
                //验证文件是否有效
                if (TryVerifyFile(activity))
                {
                    _queue.Enqueue(activity);
                    Interlocked.Increment(ref _enqueueCount);
                }
                else
                {
                    activity.IsValid = false;
                }

                EnqueueActivity(activity);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("FileScanner Enqueue", ex);
            }
            finally
            {
                _fileCount++;
            }
        }

        protected virtual void EnqueueActivity(FileActivity activity)
        {
        }

        /// <summary>
        ///文件合法性校验
        /// </summary>
        private bool TryVerifyFile(FileActivity activity)
        {
            try
            {
                return VerifyFile(activity);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("FileScanner VerifyFile", string.Format("文件路径:{0},异常详情:{1}", activity.FilePath, ex));
                return false;
            }
        }

        protected virtual bool VerifyFile(FileActivity activity)
        {
            return true;
        }

        /// <summary>
        ///目录合法性校验
        /// </summary>
        private bool TryVerifyDirectory(DirectoryInfo directoryInfo)
        {
            try
            {
                return VerifyDirectory(directoryInfo);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("FileScanner VerifyDirectory", string.Format("目录路径:{0},异常详情:{1}", directoryInfo.FullName, ex));
                return false;
            }
        }

        protected virtual bool VerifyDirectory(DirectoryInfo directoryInfo)
        {
            return true;
        }

        /// <summary>
        /// 扫描完成回调
        /// </summary>
        private void TryScanOverCallBack()
        {
            try
            {
                ScanCompleted = true;

                ScanOverCallBack();

                if (_enqueueCount == 0 || _enqueueCount == _handleCount)
                {
                    TryHandleOverCallBack();
                }
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("FileScanner ScanOverCallBack", ex);
            }
            finally
            {
                if (!_async)
                {
                    _asyncResetEvent.Set();
                }
            }
        }

        protected virtual void ScanOverCallBack()
        {
        }

        /// <summary>
        /// 处理完成回调
        /// </summary>
        private void TryHandleOverCallBack()
        {
            try
            {
                _endTime = DateTime.Now;

                Stop();
                HandleOverCallBack();
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("FileScanner HandleOverCallBack", ex);
            }
            finally
            {
                Completed = true;
            }
        }

        protected virtual void HandleOverCallBack()
        {
        }

        /// <summary>
        /// 文件监测
        /// </summary>
        /// <param name="activity"></param>
        protected void WatchFile(FileActivity activity)
        {
            _watcher.AddFile(activity.FilePath, (object sender, EventArgs args) =>
            {
                string filePath = ((string)sender).ToLower();

                FileActivity changedActivity = new FileActivity(filePath.ToLower(), true);

                TryEnqueueActivity(changedActivity);
            }, 2000, true);
        }
    }
}