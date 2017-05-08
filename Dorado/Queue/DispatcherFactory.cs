using Microsoft.Ccr.Core;
using System.Configuration;
using System.Threading;

namespace Dorado.Queue
{
    internal static class DispatcherFactory
    {
        private static object syncRoot;
        private static Dispatcher defaultDispatcher;
        private static DispatcherQueue queue;

        public static Dispatcher DefaultDispatcher
        {
            get
            {
                object obj;
                Monitor.Enter(obj = syncRoot);
                try
                {
                    if (defaultDispatcher == null)
                    {
                        int threadCount;
                        int.TryParse(ConfigurationManager.AppSettings["DefaultDispatcherThreadCount"], out threadCount);
                        if (threadCount == 0)
                        {
                            defaultDispatcher = new Dispatcher();
                        }
                        else
                        {
                            defaultDispatcher = new Dispatcher(threadCount, ThreadPriority.Highest, true, "Default Dispatcher");
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(obj);
                }
                return defaultDispatcher;
            }
        }

        static DispatcherFactory()
        {
            syncRoot = new object();
        }

        public static void DestroyDefaultDispatcher()
        {
            for (int i = 0; i < defaultDispatcher.DispatcherQueues.Count; i++)
            {
                defaultDispatcher.DispatcherQueues[i].Dispose();
            }
            defaultDispatcher.Dispose();
            defaultDispatcher = null;
        }

        public static Dispatcher CreateDispatcher(int threadCount, ThreadPriority priority, string threadPoolName)
        {
            return new Dispatcher(threadCount, priority, true, threadPoolName);
        }

        public static Dispatcher CreateDispatcher(int threadCount, string threadPoolName)
        {
            return new Dispatcher(threadCount, threadPoolName);
        }

        public static Dispatcher CreateDispatcher()
        {
            return new Dispatcher();
        }
    }
}