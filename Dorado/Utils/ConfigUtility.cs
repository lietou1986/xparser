using System;
using System.Reflection;
using System.Web.Configuration;

namespace Dorado.Utils
{
    /// <summary>
    /// 配置文件帮助类
    /// </summary>
    public sealed class ConfigUtility
    {
        public static bool IsWebApplication
        {
            get
            {
                //使用根web.config文件
                string AppVirtualPath = System.Web.HttpRuntime.AppDomainAppVirtualPath;
                if (!string.IsNullOrEmpty(AppVirtualPath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static string ApplicationName
        {
            get
            {
                string applicationName = WebConfigurationManager.AppSettings["applicationName"];

                if (string.IsNullOrEmpty(applicationName))
                    applicationName = "General";

                return applicationName;
            }
        }

        public static string ExecutablePath
        {
            get
            {
                string executablePath;

                if (IsWebApplication)
                {
                    executablePath = AppDomain.CurrentDomain.BaseDirectory;
                }
                else
                {
                    Assembly ass = Assembly.GetEntryAssembly();
                    if (ass != null)
                        executablePath = ass.Location;
                    else
                        executablePath = AppDomain.CurrentDomain.BaseDirectory;
                }

                return executablePath;
            }
        }

        public static string GetAppSetting(string key)
        {
            return WebConfigurationManager.AppSettings[key];
        }
    }
}