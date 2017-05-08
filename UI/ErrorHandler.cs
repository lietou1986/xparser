using Dorado.Core;
using System;
using System.Threading;
using System.Windows.Forms;

namespace UI
{
    public sealed class ErrorHandler
    {
        internal static void RegisterErrorHandler()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Form_UIThreadException);

            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        private static void Form_UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            Exception ex = t.Exception;
            if (ex.InnerException != null)
                ex = ex.InnerException;
            MessageBox.Show(ex.ToString());
            Log(ex);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            if (ex.InnerException != null)
                ex = ex.InnerException;
            MessageBox.Show(ex.ToString());
            Log(ex);
        }

        private static void Log(Exception ex)
        {
            LoggerWrapper.Logger.Error("XParser", ex);
        }
    }
}