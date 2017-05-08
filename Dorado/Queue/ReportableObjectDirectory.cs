using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Dorado.Queue
{
    internal static class ReportableObjectDirectory
    {
        private static Dictionary<string, IReportable> reportableObjects = new Dictionary<string, IReportable>();

        public static int ReportableCount
        {
            get
            {
                return reportableObjects.Count;
            }
        }

        public static void Add(string name, IReportable obj)
        {
            Dictionary<string, IReportable> obj2;
            Monitor.Enter(obj2 = reportableObjects);
            try
            {
                if (reportableObjects.ContainsKey(name))
                {
                    reportableObjects[name] = obj;
                }
                else
                {
                    reportableObjects.Add(name, obj);
                }
            }
            finally
            {
                Monitor.Exit(obj2);
            }
        }

        public static void Remove(string name)
        {
            Dictionary<string, IReportable> obj;
            Monitor.Enter(obj = reportableObjects);
            try
            {
                reportableObjects.Remove(name);
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public static string CreateReport()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, IReportable> obj;
            Monitor.Enter(obj = reportableObjects);
            try
            {
                foreach (IReportable reportable in reportableObjects.Values)
                {
                    sb.Append(reportable.CreateReport());
                }
            }
            finally
            {
                Monitor.Exit(obj);
            }
            return sb.ToString();
        }
    }
}