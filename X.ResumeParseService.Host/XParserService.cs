using Dorado.Core;
using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using X.ResumeParseService.Scanner;

namespace X.ResumeParseService.Host
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_STATUS
    {
        public int serviceType;
        public int currentState;
        public int controlsAccepted;
        public int win32ExitCode;
        public int serviceSpecificExitCode;
        public int checkPoint;
        public int waitHint;
    }

    public enum State
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    partial class XParserService : ServiceBase
    {
        [DllImport("ADVAPI32.DLL", EntryPoint = "SetServiceStatus")]
        public static extern bool SetServiceStatus(
                        IntPtr hServiceStatus,
                        ref SERVICE_STATUS lpServiceStatus
                        );

        private SERVICE_STATUS _serviceStatus;
        private ResumeParser _parser;

        public XParserService()
        {
            InitializeComponent();

            _parser = new ResumeParser();
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                IntPtr handle = this.ServiceHandle;
                _serviceStatus.currentState = (int)State.SERVICE_START_PENDING;
                SetServiceStatus(handle, ref _serviceStatus);

                _parser.Start();

                _serviceStatus.currentState = (int)State.SERVICE_RUNNING;
                SetServiceStatus(handle, ref _serviceStatus);

                LoggerWrapper.Logger.Info("XParserService 已经启动");
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("XParserService", ex);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
                LoggerWrapper.Logger.Error("XParserService.AppDomain.UnhandledException", ex);
        }

        protected override void OnStop()
        {
            IntPtr handle = this.ServiceHandle;
            _serviceStatus.currentState = (int)State.SERVICE_STOP_PENDING;
            SetServiceStatus(handle, ref _serviceStatus);
            _parser.Stop();
            _serviceStatus.currentState = (int)State.SERVICE_STOPPED;
            SetServiceStatus(handle, ref _serviceStatus);
            LoggerWrapper.Logger.Info("XParserService 已停止");
        }
    }
}