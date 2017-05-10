using Dorado;
using Dorado.Core;
using Dorado.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using X.DocumentExtractService.Contract.Models;
using X.ResumeParseService;
using X.ResumeParseService.Configuration;
using X.ResumeParseService.Contract;
using X.ResumeParseService.Scanner;

namespace UI
{
    public partial class Main : Form
    {
        private const string MimeType = "简历文件|*.doc;*.txt;*.docx;*.pdf;*.html;*.htm;*.rtf;*.mht";
        private const string ServiceName = "XParserService";
        private IResumeParseService _service;
        private FileScanner _fileScanner;
        private SynchronizationContext _syncContext;
        private string _currentFile;

        private static string InitialDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["testDir"] ?? "D:\\";
            }
        }

        public Main()
        {
            ErrorHandler.RegisterErrorHandler();
            InitializeComponent();
        }

        #region assist method

        private void UpdateUI(Action action)
        {
            _syncContext.Send(state =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    LoggerWrapper.Logger.Error("UpdateUI error", ex);
                }
            }, null);
        }

        /// <summary>
        /// 打印cpu信息
        /// </summary>
        private void PrintCpuInfo()
        {
            PerformanceCounter oPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            ThreadPool.QueueUserWorkItem((w) =>
            {
                while (true)
                {
                    UpdateUI(() =>
                    {
                        float cpuValue = oPerformanceCounter.NextValue();
                        txtCpuInfo.Text = cpuValue.ToString("0.0") + "%";
                    });
                    Thread.Sleep(1000);
                }
            });
        }

        private bool IsServiceRunning
        {
            get
            {
                var service = GetService();
                if (service == null) return false;
                return service.Status == ServiceControllerStatus.Running;
            }
        }

        private ServiceController GetService()
        {
            var serviceControllers = ServiceController.GetServices();
            var service = serviceControllers.FirstOrDefault(n => n.ServiceName == ServiceName);
            if (service != null)
            {
                return service;
            }
            return null;
        }

        /// <summary>
        /// 打印服务状态
        /// </summary>
        private void PrintServiceStatus()
        {
            ThreadPool.QueueUserWorkItem((w) =>
            {
                while (true)
                {
                    var service = GetService();

                    if (service != null)
                    {
                        UpdateUI(() =>
                        {
                            txtServiceStatus.Text = service.Status.ToString();
                        });
                    }
                    else
                    {
                        UpdateUI(() =>
                        {
                            txtServiceStatus.Text = "未安装";
                        });
                    }
                    Thread.Sleep(5000);
                }
            });
        }

        private void Notice(string message)
        {
            UpdateUI(() =>
            {
                this.Text = message;
            });
        }

        private void Alert(string message)
        {
            if (message.IsNullOrWhiteSpace()) return;
            lbConsole.Items.Insert(0, message);
        }

        private string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        #endregion assist method

        #region ui 操作

        private void Main_Load(object sender, EventArgs e)
        {
            _syncContext = SynchronizationContext.Current;
            _service = new ResumeParseService();
            PrintCpuInfo();
            PrintServiceStatus();
            Notice("资源加载中.....");
            ThreadPool.QueueUserWorkItem((w) =>
            {
                ResourcesConfig.Load();

                Notice("资源加载完成");

                UpdateUI(() =>
                {
                    btnDocExt.Enabled = true;
                    btnParse.Enabled = true;
                    btnResumeParse.Enabled = true;
                    btnScan.Enabled = true;
                });
            });
        }

        private void btnDocExt_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.InitialDirectory = InitialDirectory;
            fileDialog.Filter = MimeType;
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _currentFile = fileDialog.FileName;
                btnDocExt.Enabled = false;
                btnClear_Click(null, null);

                Notice(fileDialog.FileName);
                Notice("正在抽取->" + fileDialog.FileName);

                ThreadPool.QueueUserWorkItem((w) =>
                {
                    //抽取文档
                    var result = _service.Predict(fileDialog.FileName, new ExtractOption[] { ExtractOption.Text });
                    Notice("抽取完成->" + fileDialog.FileName);
                    UpdateUI(() =>
                    {
                        if (result.Status == OperateStatus.Failure)
                        {
                            Alert(result.Description);
                        }
                        else
                        {
                            txtTxt.Text = result.Data.Text;
                        }
                        btnDocExt.Enabled = true;
                    });
                });
            }
        }

        private void btnResumeParse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.InitialDirectory = InitialDirectory;
            fileDialog.Filter = MimeType;
            fileDialog.FilterIndex = 2;
            fileDialog.RestoreDirectory = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _currentFile = fileDialog.FileName;
                btnResumeParse.Enabled = false;
                btnClear_Click(null, null);
                Notice("正在解析->" + fileDialog.FileName);
                ThreadPool.QueueUserWorkItem((w) =>
                {
                    //解析简历
                    var result = _service.Parse(fileDialog.FileName, new ExtractOption[] { ExtractOption.Text });
                    FileHandleCallBack(new FileActivity(fileDialog.FileName), result);
                    UpdateUI(() =>
                    {
                        btnResumeParse.Enabled = true;
                    });
                });
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtTxt.Text = "";
            txtResult.Text = "";
            lbConsole.Items.Clear();
            txtQueueLength.Text = "0";
            txtFileCount.Text = "0";
            txtEnqueueCount.Text = "0";
            txtHandleCount.Text = "0";
            Notice("待命中...");
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            if (!txtTxt.Text.IsNullOrWhiteSpace())
            {
                btnParse.Enabled = false;
                Notice("正在解析...");
                ThreadPool.QueueUserWorkItem((w) =>
                {
                    var result = _service.ParseText(txtTxt.Text);
                    Notice("解析完成");
                    UpdateUI(() =>
                    {
                        if (result.Status == OperateStatus.Failure)
                        {
                            Alert(result.Description);
                        }
                        else
                        {
                            txtTxt.Text = result.Data.Text;
                            txtResult.Text = Serialize(result.Data.ResumeInfo);
                        }

                        btnParse.Enabled = true;
                    });
                });
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (IsServiceRunning)
            {
                MessageBox.Show("已有后台服务在运行，请先停止服务！");
                return;
            }
            List<string> directorys = new List<string>();
            DialogResult select = MessageBox.Show("要执行全盘扫描吗?", "扫描确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (select == DialogResult.Yes)
            {
                List<DriveInfo> drives = Directory.GetLogicalDrives().Select(q => new DriveInfo(q)).ToList();
                foreach (var driveInfo in drives)
                {
                    if (driveInfo.IsReady && driveInfo.DriveType == DriveType.Fixed)
                    {
                        directorys.Add(driveInfo.Name);
                    }
                }
                if (directorys.Count == 0)
                {
                    Notice("无有效磁盘驱动器");
                    return;
                }
            }
            else if (select == DialogResult.No)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择目录";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string foldPath = dialog.SelectedPath;
                    directorys.Add(foldPath);
                    Notice("待扫描目录：" + foldPath);
                }
            }

            ThreadPool.QueueUserWorkItem((w) =>
            {
                UpdateUI(() =>
                {
                    Notice("等待初始化队列...");
                });
                _fileScanner = new ResumeFileScanner(directorys, ckAsync.Checked, Convert.ToInt32(numTheadCount.Value), FileEnqueueCallBack, FileHandleCallBack, ScanOverCallBack, HandleOverCallBack);
                UpdateUI(() =>
                    {
                        Notice("队列创建就绪");
                        btnStart.Enabled = true;
                    });
            });
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_fileScanner != null && _fileScanner.Completed)
            {
                MessageBox.Show("本次扫描已结束，请重新选择待扫描的目录");
                return;
            }
            btnStop.Enabled = true;
            btnStart.Enabled = false;
            btnScan.Enabled = false;
            btnClear_Click(null, null);
            ThreadPool.QueueUserWorkItem((w) =>
            {
                _fileScanner.Start();
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            btnScan.Enabled = true;

            _fileScanner.Stop();
        }

        private void lblEnqueueStatus_Click(object sender, EventArgs e)
        {
            if (_currentFile.IsNullOrWhiteSpace())
                return;
            if (!File.Exists(_currentFile))
                return;

            Process.Start(_currentFile);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion ui 操作

        #region 扫描器回调

        private void PrintProcessInfo()
        {
            UpdateUI(() =>
            {
                txtQueueLength.Text = _fileScanner.QueueLength.ToString();
                txtFileCount.Text = _fileScanner.FileCount.ToString();
                txtEnqueueCount.Text = _fileScanner.EnqueueCount.ToString();
                txtHandleCount.Text = _fileScanner.HandleCount.ToString();
                txtTotalTime.Text = _fileScanner.TotalTime.ToString("f2") + "s";
            });
        }

        private void FileEnqueueCallBack(FileActivity activity)
        {
            lblEnqueueStatus.Text = "扫描文件->" + activity.FilePath;

            PrintProcessInfo();

            UpdateUI(() =>
            {
                if (activity.IsChanged)
                    Alert(string.Format("文件发生变化->{0}", activity.FilePath));
                if (!activity.IsValid)
                    Alert(string.Format("文件无效->{0} {1}", activity.Message, activity.FilePath));
            });
        }

        private void FileHandleCallBack(FileActivity activity, OperateResult<ResumeResult> result)
        {
            _currentFile = activity.FilePath;

            PrintProcessInfo();

            UpdateUI(() =>
           {
               Notice("解析完成->" + _currentFile);

               if (result.Status == OperateStatus.Failure)
               {
                   Alert(result.Description);
                   if (result.Data != null)
                   {
                       txtTxt.Text = result.Data.Text;
                   }
               }
               else
               {
                   txtTxt.Text = result.Data.Text;
                   txtResult.Text = Serialize(result.Data.ResumeInfo);
               }
           });
        }

        private void ScanOverCallBack()
        {
            UpdateUI(() =>
            {
                PrintProcessInfo();
                lblEnqueueStatus.Text = string.Format("扫描完成！ 共扫描文件[{0}]，有效入队文件[{1}]，耗时[{2}s]", _fileScanner.FileCount, _fileScanner.EnqueueCount, _fileScanner.TotalTime.ToString("f2"));
            });
        }

        private void HandleOverCallBack()
        {
            UpdateUI(() =>
            {
                btnStop_Click(null, null);
                PrintProcessInfo();
                lblEnqueueStatus.Text = string.Format("处理完成！ 共扫描文件[{0}]，有效入队文件[{1}]，处理文件[{2}]，耗时[{3}s]", _fileScanner.FileCount, _fileScanner.EnqueueCount, _fileScanner.HandleCount, _fileScanner.TotalTime.ToString("f2"));
            });
        }

        #endregion 扫描器回调
    }
}