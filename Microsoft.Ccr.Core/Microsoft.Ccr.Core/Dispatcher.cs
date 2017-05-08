using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public sealed class Dispatcher : IDisposable
    {
        private const int CausalityTableMaximumSize = 1024;

        private static bool _causalitiesActive;

        private static Dictionary<int, CausalityThreadContext> _causalityTable = new Dictionary<int, CausalityThreadContext>(1024);

        internal static readonly TraceSwitch TraceSwitchCore = new TraceSwitch("Microsoft.Ccr.Core", "Ccr.Core debug switch");

        internal int _workerCount;

        internal ManualResetEvent _startupCompleteEvent = new ManualResetEvent(false);

        internal List<DispatcherQueue> _dispatcherQueues = new List<DispatcherQueue>();

        internal int _cachedDispatcherQueueCount;

        internal List<TaskExecutionWorker> _taskExecutionWorkers = new List<TaskExecutionWorker>();

        private int _cachedWorkerListCount;

        private Dictionary<string, DispatcherQueue> _nameToQueueTable = new Dictionary<string, DispatcherQueue>();

        private static int _threadsPerCpu = 1;

        private static int NumberOfProcessorsInternal = Dispatcher.GetNumberOfProcessors();

        private string _name;

        private DispatcherOptions _options;

        private Port<Exception> _unhandledPort;

        internal volatile int _pendingTaskCount;

        internal volatile int _suspendedQueueCount;

        private bool _hasShutdown;

        public event UnhandledExceptionEventHandler UnhandledException;

        public static ICollection<ICausality> ActiveCausalities
        {
            get
            {
                CausalityThreadContext currentThreadCausalities = Dispatcher.GetCurrentThreadCausalities();
                if (CausalityThreadContext.IsEmpty(currentThreadCausalities))
                {
                    return new Causality[0];
                }
                return currentThreadCausalities.Causalities;
            }
        }

        public static bool HasActiveCausalities
        {
            get
            {
                CausalityThreadContext currentThreadCausalities = Dispatcher.GetCurrentThreadCausalities();
                return !CausalityThreadContext.IsEmpty(currentThreadCausalities);
            }
        }

        public int PendingTaskCount
        {
            get
            {
                return _pendingTaskCount;
            }
            set
            {
            }
        }

        public long ProcessedTaskCount
        {
            get
            {
                long num = 0L;
                lock (_nameToQueueTable)
                {
                    for (int i = 0; i < _dispatcherQueues.Count; i++)
                    {
                        num += _dispatcherQueues[i].ScheduledTaskCount;
                    }
                }
                return num;
            }
            set
            {
            }
        }

        public int WorkerThreadCount
        {
            get
            {
                return _workerCount;
            }
            set
            {
            }
        }

        public static int ThreadsPerCpu
        {
            get
            {
                return Dispatcher._threadsPerCpu;
            }
            set
            {
                Dispatcher._threadsPerCpu = value;
            }
        }

        public DispatcherOptions Options
        {
            get
            {
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Port<Exception> UnhandledExceptionPort
        {
            get
            {
                return _unhandledPort;
            }
            set
            {
                _unhandledPort = value;
            }
        }

        public List<DispatcherQueue> DispatcherQueues
        {
            get
            {
                return new List<DispatcherQueue>(_nameToQueueTable.Values);
            }
        }

        internal static void AddThread(Thread thread)
        {
            lock (Dispatcher._causalityTable)
            {
                Dispatcher._causalityTable[thread.ManagedThreadId] = null;
            }
        }

        internal static void SetCurrentThreadCausalities(CausalityThreadContext context)
        {
            if (!Dispatcher._causalitiesActive)
            {
                return;
            }
            try
            {
                Dispatcher._causalityTable[Thread.CurrentThread.ManagedThreadId] = context;
            }
            catch (Exception)
            {
                if (!Dispatcher._causalityTable.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                {
                    try
                    {
                        Dispatcher.AddThread(Thread.CurrentThread);
                        Dispatcher._causalityTable[Thread.CurrentThread.ManagedThreadId] = context;
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static CausalityThreadContext CloneCausalitiesFromCurrentThread()
        {
            if (!Dispatcher._causalitiesActive)
            {
                return null;
            }
            CausalityThreadContext currentThreadCausalities = Dispatcher.GetCurrentThreadCausalities();
            if (CausalityThreadContext.IsEmpty(currentThreadCausalities))
            {
                return null;
            }
            return currentThreadCausalities.Clone();
        }

        internal static CausalityThreadContext GetCurrentThreadCausalities()
        {
            if (!Dispatcher._causalitiesActive)
            {
                return null;
            }
            CausalityThreadContext result;
            Dispatcher._causalityTable.TryGetValue(Thread.CurrentThread.ManagedThreadId, out result);
            return result;
        }

        public static void AddCausality(ICausality causality)
        {
            Dispatcher._causalitiesActive = true;
            CausalityThreadContext causalityThreadContext = Dispatcher.GetCurrentThreadCausalities();
            if (CausalityThreadContext.IsEmpty(causalityThreadContext))
            {
                causalityThreadContext = new CausalityThreadContext(causality, null);
                Dispatcher.SetCurrentThreadCausalities(causalityThreadContext);
                return;
            }
            causalityThreadContext.AddCausality(causality);
        }

        public static void AddCausalityBreak()
        {
            Dispatcher.AddCausality(new Causality("BreakingCausality")
            {
                BreakOnReceive = true
            });
        }

        public static bool RemoveCausality(ICausality causality)
        {
            return Dispatcher.RemoveCausality(null, causality);
        }

        public static void ClearCausalities()
        {
            Dispatcher.SetCurrentThreadCausalities(null);
        }

        public static bool RemoveCausality(string name)
        {
            return Dispatcher.RemoveCausality(name, null);
        }

        private static bool RemoveCausality(string name, ICausality causality)
        {
            CausalityThreadContext currentThreadCausalities = Dispatcher.GetCurrentThreadCausalities();
            return !CausalityThreadContext.IsEmpty(currentThreadCausalities) && currentThreadCausalities.RemoveCausality(name, causality);
        }

        internal static void TransferCausalitiesFromTaskToCurrentThread(ITask currentTask)
        {
            if (!Dispatcher._causalitiesActive)
            {
                return;
            }
            CausalityThreadContext causalityThreadContext = null;
            for (int i = 0; i < currentTask.PortElementCount; i++)
            {
                IPortElement portElement = currentTask[i];
                if (portElement != null && portElement.CausalityContext != null)
                {
                    CausalityThreadContext context = (CausalityThreadContext)portElement.CausalityContext;
                    if (causalityThreadContext == null)
                    {
                        causalityThreadContext = new CausalityThreadContext(null, null);
                    }
                    causalityThreadContext.MergeWith(context);
                }
            }
            Dispatcher.SetCurrentThreadCausalities(causalityThreadContext);
        }

        internal static void FilterExceptionThroughCausalities(ITask task, Exception exception)
        {
            try
            {
                CausalityThreadContext currentThreadCausalities = Dispatcher.GetCurrentThreadCausalities();
                if (CausalityThreadContext.IsEmpty(currentThreadCausalities))
                {
                    if (task != null)
                    {
                        DispatcherQueue taskQueue = task.TaskQueue;
                        if (!taskQueue.RaiseUnhandledException(exception))
                        {
                            Dispatcher dispatcher = taskQueue.Dispatcher;
                            if (dispatcher != null)
                            {
                                dispatcher.RaiseUnhandledException(exception);
                            }
                        }
                    }
                }
                else
                {
                    currentThreadCausalities.PostException(exception);
                }
            }
            catch (Exception exception2)
            {
                Dispatcher.LogError(Resource1.ExceptionDuringCausalityHandling, exception2);
            }
        }

        private static int GetNumberOfProcessors()
        {
            int result = 1;
            try
            {
                if (Dispatcher.TraceSwitchCore.TraceInfo)
                {
                    Trace.WriteLine("CCR Dispatcher: Processors:" + Environment.ProcessorCount);
                }
                result = Environment.ProcessorCount;
            }
            catch (Exception arg)
            {
                Trace.WriteLine("CCR Dispatcher: Exception reading processor count:" + arg);
            }
            return result;
        }

        private void RaiseUnhandledException(Exception e)
        {
            if (_unhandledPort != null)
            {
                _unhandledPort.Post(e);
            }
            if (UnhandledException != null)
            {
                ThreadPool.QueueUserWorkItem(delegate (object state)
                {
                    UnhandledException(this, new UnhandledExceptionEventArgs(state as Exception, false));
                }, e);
            }
        }

        public Dispatcher() : this(0, null)
        {
        }

        public Dispatcher(int threadCount, string threadPoolName) : this(threadCount, ThreadPriority.Normal, DispatcherOptions.None, ApartmentState.Unknown, threadPoolName)
        {
        }

        public Dispatcher(int threadCount, ThreadPriority priority, bool useBackgroundThreads, string threadPoolName) : this(threadCount, priority, useBackgroundThreads ? DispatcherOptions.UseBackgroundThreads : DispatcherOptions.None, ApartmentState.Unknown, threadPoolName)
        {
        }

        public Dispatcher(int threadCount, ThreadPriority priority, DispatcherOptions options, string threadPoolName) : this(threadCount, priority, options, ApartmentState.Unknown, 0, threadPoolName)
        {
        }

        public Dispatcher(int threadCount, ThreadPriority priority, DispatcherOptions options, ApartmentState threadApartmentState, string threadPoolName) : this(threadCount, priority, options, threadApartmentState, 0, threadPoolName)
        {
        }

        public Dispatcher(int threadCount, ThreadPriority priority, DispatcherOptions options, ApartmentState threadApartmentState, int maxThreadStackSize, string threadPoolName)
        {
            if (threadCount == 0)
            {
                threadCount = Math.Max(Dispatcher.NumberOfProcessorsInternal, 2) * Dispatcher.ThreadsPerCpu;
            }
            else if (threadCount < 0)
            {
                throw new ArgumentException("Cannot create a negative number of threads. Pass 0 to use default.", "threadCount");
            }
            if (threadPoolName == null)
            {
                _name = string.Empty;
            }
            else
            {
                _name = threadPoolName;
            }
            _options = options;
            for (int i = 0; i < threadCount; i++)
            {
                AddWorker(priority, threadApartmentState, maxThreadStackSize);
            }
            StartWorkers();
        }

        private static void SetWorkerThreadAffinity(DateTime dispatcherStartTime)
        {
            try
            {
                int num = 0;
                TimeSpan t = TimeSpan.FromMilliseconds(100.0);
                foreach (ProcessThread processThread in Process.GetCurrentProcess().Threads)
                {
                    if (!(processThread.StartTime - dispatcherStartTime > t) && !(processThread.StartTime < dispatcherStartTime) && !(processThread.TotalProcessorTime > t))
                    {
                        IntPtr processorAffinity = new IntPtr(1 << num++ % Dispatcher.NumberOfProcessorsInternal);
                        processThread.ProcessorAffinity = processorAffinity;
                    }
                }
            }
            catch (Exception exception)
            {
                Dispatcher.LogError("Could not set thread affinity", exception);
            }
        }

        internal void AddQueue(string queueName, DispatcherQueue queue)
        {
            lock (_nameToQueueTable)
            {
                _nameToQueueTable.Add(queueName, queue);
                _dispatcherQueues.Add(queue);
                _cachedDispatcherQueueCount++;
            }
        }

        internal bool RemoveQueue(string queueName)
        {
            bool result;
            lock (_nameToQueueTable)
            {
                DispatcherQueue item;
                if (!_nameToQueueTable.TryGetValue(queueName, out item))
                {
                    result = false;
                }
                else
                {
                    _nameToQueueTable.Remove(queueName);
                    _dispatcherQueues.Remove(item);
                    _cachedDispatcherQueueCount--;
                    result = true;
                }
            }
            return result;
        }

        private void AddWorker(ThreadPriority priority, ApartmentState apartmentState, int maxThreadStackSize)
        {
            TaskExecutionWorker taskExecutionWorker = new TaskExecutionWorker(this);
            Thread thread = new Thread(new ThreadStart(taskExecutionWorker.ExecutionLoop), maxThreadStackSize);
            thread.SetApartmentState(apartmentState);
            thread.Name = _name;
            thread.Priority = priority;
            thread.IsBackground = (DispatcherOptions.None < (_options & DispatcherOptions.UseBackgroundThreads));
            taskExecutionWorker._thread = thread;
            _taskExecutionWorkers.Add(taskExecutionWorker);
            _cachedWorkerListCount++;
        }

        private void StartWorkers()
        {
            DateTime now = DateTime.Now;
            foreach (TaskExecutionWorker current in _taskExecutionWorkers)
            {
                current._thread.Start();
            }
            _startupCompleteEvent.WaitOne();
            if ((_options & DispatcherOptions.UseProcessorAffinity) > DispatcherOptions.None)
            {
                Dispatcher.SetWorkerThreadAffinity(now);
            }
            _startupCompleteEvent.Close();
            _startupCompleteEvent = null;
            foreach (TaskExecutionWorker current2 in _taskExecutionWorkers)
            {
                Dispatcher.AddThread(current2._thread);
            }
        }

        internal void Signal()
        {
            if (_cachedWorkerListCount == 0)
            {
                Dispatcher.LogError("Dispatcher disposed, will not schedule task", new ObjectDisposedException("Dispatcher"));
                return;
            }
            Interlocked.Increment(ref _pendingTaskCount);
            for (int i = 0; i < _cachedWorkerListCount; i++)
            {
                TaskExecutionWorker taskExecutionWorker = _taskExecutionWorkers[i];
                if (taskExecutionWorker.Signal())
                {
                    return;
                }
            }
        }

        internal void QueueSuspendNotification()
        {
            Interlocked.Increment(ref _suspendedQueueCount);
        }

        internal void QueueResumeNotification()
        {
            if (Interlocked.Decrement(ref _suspendedQueueCount) < 0)
            {
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            if (_cachedWorkerListCount == 0)
            {
                return;
            }
            lock (_taskExecutionWorkers)
            {
                foreach (TaskExecutionWorker current in _taskExecutionWorkers)
                {
                    current.Shutdown();
                }
                _cachedWorkerListCount = 0;
            }
            if (_startupCompleteEvent != null)
            {
                _startupCompleteEvent.Close();
            }
            Shutdown(true);
        }

        private void Shutdown(bool wait)
        {
            Dispose();
            lock (_taskExecutionWorkers)
            {
                _hasShutdown = true;
                Monitor.PulseAll(_taskExecutionWorkers);
                if (wait)
                {
                    while (!_hasShutdown)
                    {
                        Monitor.Wait(_taskExecutionWorkers);
                    }
                }
            }
        }

        internal void AdjustPendingCount(int count)
        {
            Interlocked.Add(ref _pendingTaskCount, count);
        }

        internal static void LogError(string message, Exception exception)
        {
            string message2 = string.Format(CultureInfo.InvariantCulture, "*** {0}: Exception:{1}", new object[]
            {
                message,
                exception
            });
            if (Dispatcher.TraceSwitchCore.TraceError)
            {
                Trace.WriteLine(message2);
            }
        }

        internal static void LogInfo(string message)
        {
            if (Dispatcher.TraceSwitchCore.TraceInfo)
            {
                Trace.WriteLine("*    " + message);
            }
        }
    }
}