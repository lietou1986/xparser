using log4net;
using System;

namespace Dorado.Core
{
    public class LoggerWrapper
    {
        private LoggerWrapper()
        {
        }

        public static readonly LoggerWrapper Logger = new LoggerWrapper();

        private static readonly ILog logInfo = LogManager.GetLogger("Info");

        private static readonly ILog logWarn = LogManager.GetLogger("Warn");

        private static readonly ILog logDebug = LogManager.GetLogger("Debug");

        private static readonly ILog logError = LogManager.GetLogger("Error");

        public void Info(string format, params object[] args)
        {
            if (logInfo.IsInfoEnabled)
            {
                logInfo.InfoFormat(format);
            }
        }

        public void Debug(string format, params object[] args)
        {
            if (logDebug.IsDebugEnabled)
            {
                logDebug.DebugFormat(format);
            }
        }

        public void Warn(string format, params object[] args)
        {
            if (logWarn.IsWarnEnabled)
            {
                logWarn.WarnFormat(format);
            }
        }

        public void Error(string message, Exception ex)
        {
            if (logError.IsErrorEnabled)
            {
                logError.Error(message, ex);
            }
        }

        public void Error(string message, params object[] args)
        {
            if (logError.IsErrorEnabled)
            {
                logError.ErrorFormat(message, args);
            }
        }
    }
}