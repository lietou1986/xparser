using System;
using System.IO;
using System.ServiceProcess;

namespace X.ResumeParseService.Host
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        private static void Main()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new XParserService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}