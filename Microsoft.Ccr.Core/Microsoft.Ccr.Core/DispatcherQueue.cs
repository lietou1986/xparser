using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Ccr.Core
{
    public class DispatcherQueue : IDisposable
    {
        private class ClrSystemTimerContext
        {
            public long Id;

            public Timer Timer;

            public Port<DateTime> TimerPort;

            public CausalityThreadContext CausalityContext;

            public ClrSystemTimerContext(Port<DateTime> timerPort, CausalityThreadContext causalityContext)
            {
                CausalityContext = causalityContext;
                lock (typeof(DispatcherQueue.ClrSystemTimerContext))
                {
                    DispatcherQueue._timerContextIdentifier += 1L;
                    Id = DispatcherQueue._timerContextIdentifier;
                }
                TimerPort = timerPort;
            }
        }

        private class TimerContext
        {
            public Port<DateTime> TimerPort;

            public CausalityThreadContext CausalityContext;

            public DateTime Expiration;

            public TimerContext(Port<DateTime> timerPort, CausalityThreadContext causalityContext, DateTime expiration)
            {
                CausalityContext = causalityContext;
                TimerPort = timerPort;
                Expiration = expiration;
            }
        }

        private string _name;

        private Queue<ITask> _taskQueue = new Queue<ITask>();

        private TaskCommon _taskCommonListHead;

        private bool _isDisposed;

        private bool _isSuspended;

        internal Dispatcher _dispatcher;

        private long _scheduledTaskCount;

        private TaskExecutionPolicy _policy;

        private Stopwatch _watch;

        private int _maximumQueueDepth;

        private double _currentSchedulingRate;

        private double _scheduledItems;

        private double _maximumSchedulingRate;

        private double _timescale = 1.0;

        private Port<ITask> _policyNotificationPort;

        private TimeSpan _throttlingSleepInterval = TimeSpan.FromMilliseconds(10.0);

        private static long _timerContextIdentifier;

        private Dictionary<long, Timer> _clrSystemTimerTable = new Dictionary<long, Timer>();

        private SortedList<long, List<DispatcherQueue.TimerContext>> _timerTable = new SortedList<long, List<DispatcherQueue.TimerContext>>();

        private DateTime _nextTimerExpiration = DateTime.UtcNow + TimeSpan.FromDays(1.0);

        private volatile int _taskCommonCount;

        private Port<Exception> _unhandledPort;

        public event UnhandledExceptionEventHandler UnhandledException;

        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
            set
            {
                _isDisposed = value;
            }
        }

        public bool IsSuspended
        {
            get
            {
                return _isSuspended;
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

        public bool IsUsingThreadPool
        {
            get
            {
                return _dispatcher == null;
            }
            set
            {
                if (_dispatcher != null && !value)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        [XmlIgnore]
        public Dispatcher Dispatcher
        {
            get
            {
                return _dispatcher;
            }
        }

        public int Count
        {
            get
            {
                return _taskQueue.Count + _taskCommonCount;
            }
            set
            {
            }
        }

        public long ScheduledTaskCount
        {
            get
            {
                return _scheduledTaskCount;
            }
            set
            {
            }
        }

        public TaskExecutionPolicy Policy
        {
            get
            {
                return _policy;
            }
            set
            {
                if (value != TaskExecutionPolicy.Unconstrained && _watch == null)
                {
                    _watch = Stopwatch.StartNew();
                }
                _policy = value;
            }
        }

        public int MaximumQueueDepth
        {
            get
            {
                return _maximumQueueDepth;
            }
            set
            {
                _maximumQueueDepth = value;
            }
        }

        public double CurrentSchedulingRate
        {
            get
            {
                return _currentSchedulingRate;
            }
            set
            {
                _currentSchedulingRate = value;
            }
        }

        public double MaximumSchedulingRate
        {
            get
            {
                return _maximumSchedulingRate;
            }
            set
            {
                _maximumSchedulingRate = value;
            }
        }

        public double Timescale
        {
            get
            {
                return _timescale;
            }
            set
            {
                _timescale = value;
            }
        }

        [XmlIgnore]
        public Port<ITask> ExecutionPolicyNotificationPort
        {
            get
            {
                return _policyNotificationPort;
            }
            set
            {
                _policyNotificationPort = value;
            }
        }

        [XmlIgnore]
        public TimeSpan ThrottlingSleepInterval
        {
            get
            {
                return _throttlingSleepInterval;
            }
            set
            {
                _throttlingSleepInterval = value;
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

        public DispatcherQueue()
        {
            _name = "Unnamed queue using CLR Threadpool";
        }

        public DispatcherQueue(string name)
        {
            _name = name;
        }

        public DispatcherQueue(string name, Dispatcher dispatcher) : this(name, dispatcher, TaskExecutionPolicy.Unconstrained, 0, 1.0)
        {
        }

        public DispatcherQueue(string name, Dispatcher dispatcher, TaskExecutionPolicy policy, int maximumQueueDepth) : this(name, dispatcher, policy, maximumQueueDepth, 0.0)
        {
        }

        public DispatcherQueue(string name, Dispatcher dispatcher, TaskExecutionPolicy policy, double schedulingRate) : this(name, dispatcher, policy, 0, schedulingRate)
        {
        }

        private DispatcherQueue(string name, Dispatcher dispatcher, TaskExecutionPolicy policy, int maximumQueueDepth, double schedulingRate)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }
            if ((policy == TaskExecutionPolicy.ConstrainQueueDepthDiscardTasks || policy == TaskExecutionPolicy.ConstrainQueueDepthThrottleExecution) && maximumQueueDepth <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumQueueDepth");
            }
            if ((policy == TaskExecutionPolicy.ConstrainSchedulingRateDiscardTasks || policy == TaskExecutionPolicy.ConstrainSchedulingRateThrottleExecution) && schedulingRate <= 0.0)
            {
                throw new ArgumentOutOfRangeException("schedulingRate");
            }
            _dispatcher = dispatcher;
            _name = name;
            _policy = policy;
            _maximumQueueDepth = maximumQueueDepth;
            _maximumSchedulingRate = schedulingRate;
            dispatcher.AddQueue(name, this);
            if (policy != TaskExecutionPolicy.Unconstrained)
            {
                _watch = Stopwatch.StartNew();
            }
        }

        protected virtual Timer EnqueueTimerUsingClrSystemTimers(TimeSpan timeSpan, Port<DateTime> timerPort)
        {
            CausalityThreadContext causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
            if (timeSpan.TotalMilliseconds <= 5.0 && timeSpan.Milliseconds >= 0)
            {
                timerPort.Post(DateTime.Now);
                return null;
            }
            timeSpan = TimeSpan.FromSeconds(timeSpan.TotalSeconds * _timescale);
            DispatcherQueue.ClrSystemTimerContext clrSystemTimerContext = new DispatcherQueue.ClrSystemTimerContext(timerPort, causalityContext);
            Timer timer = new Timer(new TimerCallback(ClrSystemTimerHandler), clrSystemTimerContext, -1, -1);
            clrSystemTimerContext.Timer = timer;
            lock (_clrSystemTimerTable)
            {
                _clrSystemTimerTable.Add(clrSystemTimerContext.Id, timer);
            }
            timer.Change(timeSpan, TimeSpan.FromMilliseconds(-1.0));
            return timer;
        }

        private void ClrSystemTimerHandler(object state)
        {
            DispatcherQueue.ClrSystemTimerContext clrSystemTimerContext = (DispatcherQueue.ClrSystemTimerContext)state;
            try
            {
                lock (_clrSystemTimerTable)
                {
                    _clrSystemTimerTable.Remove(clrSystemTimerContext.Id);
                }
                clrSystemTimerContext.Timer.Dispose();
                Dispatcher.SetCurrentThreadCausalities(clrSystemTimerContext.CausalityContext);
                clrSystemTimerContext.TimerPort.Post(DateTime.Now);
            }
            catch (Exception exception)
            {
                Dispatcher.LogError("DispatcherQueue:TimerHandler", exception);
            }
        }

        public virtual void EnqueueTimer(TimeSpan timeSpan, Port<DateTime> timerPort)
        {
            if (_dispatcher == null || (_dispatcher.Options & DispatcherOptions.UseHighAccuracyTimerLogic) == DispatcherOptions.None)
            {
                EnqueueTimerUsingClrSystemTimers(timeSpan, timerPort);
                return;
            }
            CausalityThreadContext causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
            timeSpan = TimeSpan.FromSeconds(timeSpan.TotalSeconds * _timescale);
            DateTime dateTime = DateTime.UtcNow + timeSpan;
            DispatcherQueue.TimerContext item = new DispatcherQueue.TimerContext(timerPort, causalityContext, dateTime);
            bool flag = false;
            lock (_timerTable)
            {
                if (dateTime < _nextTimerExpiration)
                {
                    _nextTimerExpiration = dateTime;
                    flag = true;
                }
                if (_timerTable.ContainsKey(dateTime.Ticks))
                {
                    _timerTable[dateTime.Ticks].Add(item);
                }
                else
                {
                    List<DispatcherQueue.TimerContext> list = new List<DispatcherQueue.TimerContext>(1);
                    list.Add(item);
                    _timerTable[dateTime.Ticks] = list;
                }
            }
            if (flag)
            {
                Enqueue(new Task(delegate
                {
                }));
            }
        }

        internal bool CheckTimerExpirations()
        {
            if (_timerTable.Count == 0 || _isDisposed || _isSuspended)
            {
                return false;
            }
            if (DateTime.UtcNow < _nextTimerExpiration)
            {
                return true;
            }
            List<DispatcherQueue.TimerContext> list = null;
            while (true)
            {
                IL_35:
                lock (_timerTable)
                {
                    foreach (List<DispatcherQueue.TimerContext> current in _timerTable.Values)
                    {
                        if (current[0].Expiration <= DateTime.UtcNow)
                        {
                            if (list == null)
                            {
                                list = new List<DispatcherQueue.TimerContext>();
                            }
                            list.AddRange(current);
                            _timerTable.Remove(current[0].Expiration.Ticks);
                            goto IL_35;
                        }
                    }
                    if (_timerTable.Count == 0)
                    {
                        _nextTimerExpiration = DateTime.UtcNow.AddDays(1.0);
                    }
                    else
                    {
                        using (IEnumerator<List<DispatcherQueue.TimerContext>> enumerator2 = _timerTable.Values.GetEnumerator())
                        {
                            if (enumerator2.MoveNext())
                            {
                                List<DispatcherQueue.TimerContext> current2 = enumerator2.Current;
                                _nextTimerExpiration = current2[0].Expiration;
                            }
                        }
                    }
                }
                break;
            }
            if (list != null)
            {
                foreach (DispatcherQueue.TimerContext current3 in list)
                {
                    SignalTimer(current3);
                }
            }
            return true;
        }

        private void SignalTimer(DispatcherQueue.TimerContext tc)
        {
            try
            {
                Dispatcher.SetCurrentThreadCausalities(tc.CausalityContext);
                tc.TimerPort.Post(DateTime.Now);
                Dispatcher.ClearCausalities();
            }
            catch (Exception exception)
            {
                Dispatcher.LogError("DispatcherQueue:TimerHandler", exception);
            }
        }

        private void TaskListAddLast(TaskCommon Item)
        {
            if (_taskCommonListHead == null)
            {
                _taskCommonListHead = Item;
                Item._next = Item;
                Item._previous = Item;
            }
            else
            {
                _taskCommonListHead._previous._next = Item;
                Item._previous = _taskCommonListHead._previous;
                Item._next = _taskCommonListHead;
                _taskCommonListHead._previous = Item;
            }
            _taskCommonCount++;
        }

        private TaskCommon TaskListRemoveFirst()
        {
            if (_taskCommonListHead == null)
            {
                return null;
            }
            if (object.ReferenceEquals(_taskCommonListHead._next, _taskCommonListHead))
            {
                TaskCommon taskCommonListHead = _taskCommonListHead;
                _taskCommonListHead = null;
                _taskCommonCount--;
                taskCommonListHead._next = null;
                taskCommonListHead._previous = null;
                return taskCommonListHead;
            }
            TaskCommon taskCommonListHead2 = _taskCommonListHead;
            _taskCommonListHead = _taskCommonListHead._next;
            _taskCommonListHead._previous = taskCommonListHead2._previous;
            _taskCommonListHead._previous._next = _taskCommonListHead;
            _taskCommonCount--;
            taskCommonListHead2._next = null;
            taskCommonListHead2._previous = null;
            return taskCommonListHead2;
        }

        public virtual bool Enqueue(ITask task)
        {
            ITask task2 = null;
            bool flag = false;
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            task.TaskQueue = this;
            if (_dispatcher == null)
            {
                _scheduledTaskCount += 1L;
                ThreadPool.QueueUserWorkItem(new WaitCallback(TaskExecutionWorker.ExecuteInCurrentThreadContext), task);
                return task2 == null;
            }
            lock (_taskQueue)
            {
                if (_isDisposed)
                {
                    if ((_dispatcher.Options & DispatcherOptions.SuppressDisposeExceptions) == DispatcherOptions.None)
                    {
                        throw new ObjectDisposedException(typeof(DispatcherQueue).Name + ":" + Name);
                    }
                    return false;
                }
                else
                {
                    switch (_policy)
                    {
                        case TaskExecutionPolicy.Unconstrained:
                            {
                                TaskCommon taskCommon = task as TaskCommon;
                                if (taskCommon != null)
                                {
                                    TaskListAddLast(taskCommon);
                                }
                                else
                                {
                                    _taskQueue.Enqueue(task);
                                }
                                break;
                            }
                        case TaskExecutionPolicy.ConstrainQueueDepthDiscardTasks:
                            RecalculateSchedulingRate();
                            if (_taskQueue.Count >= _maximumQueueDepth)
                            {
                                Dispatcher.LogInfo("DispatcherQueue.Enqueue: Discarding oldest task because queue depth limit reached");
                                ITask task3;
                                TryDequeue(out task3);
                                task2 = task3;
                            }
                            _taskQueue.Enqueue(task);
                            break;

                        case TaskExecutionPolicy.ConstrainQueueDepthThrottleExecution:
                            RecalculateSchedulingRate();
                            if (_taskQueue.Count >= _maximumQueueDepth)
                            {
                                Dispatcher.LogInfo("DispatcherQueue.Enqueue: Forcing thread sleep because queue depth limit reached");
                                while (_taskQueue.Count >= _maximumQueueDepth)
                                {
                                    Sleep();
                                }
                                flag = true;
                            }
                            _taskQueue.Enqueue(task);
                            break;

                        case TaskExecutionPolicy.ConstrainSchedulingRateDiscardTasks:
                            RecalculateSchedulingRate();
                            if (_currentSchedulingRate >= _maximumSchedulingRate)
                            {
                                Dispatcher.LogInfo("DispatcherQueue.Enqueue: Discarding task because task scheduling rate exceeded");
                                ITask task4;
                                TryDequeue(out task4);
                                task2 = task4;
                            }
                            _scheduledItems += 1.0;
                            _taskQueue.Enqueue(task);
                            break;

                        case TaskExecutionPolicy.ConstrainSchedulingRateThrottleExecution:
                            RecalculateSchedulingRate();
                            if (_currentSchedulingRate >= _maximumSchedulingRate)
                            {
                                Dispatcher.LogInfo("DispatcherQueue.Enqueue: Forcing thread sleep because task scheduling rate exceeded");
                                while (_currentSchedulingRate > _maximumSchedulingRate)
                                {
                                    Sleep();
                                    RecalculateSchedulingRate();
                                }
                                flag = true;
                            }
                            _scheduledItems += 1.0;
                            _taskQueue.Enqueue(task);
                            break;
                    }
                    _scheduledTaskCount += 1L;
                    SignalDispatcher();
                }
            }
            if (task2 != null || flag)
            {
                TaskExecutionPolicyEngaged(task2, flag);
            }
            return task2 == null;
        }

        protected virtual void SignalDispatcher()
        {
            _dispatcher.Signal();
        }

        private void Sleep()
        {
            Monitor.Exit(_taskQueue);
            Thread.Sleep((int)_throttlingSleepInterval.TotalMilliseconds);
            Monitor.Enter(_taskQueue);
        }

        private void TaskExecutionPolicyEngaged(ITask task, bool throttlingEnabled)
        {
            if (!throttlingEnabled)
            {
                Interlocked.Decrement(ref _dispatcher._pendingTaskCount);
            }
            if (task != null && task.ArbiterCleanupHandler != null)
            {
                task.ArbiterCleanupHandler();
            }
            Port<ITask> policyNotificationPort = _policyNotificationPort;
            if (policyNotificationPort != null)
            {
                policyNotificationPort.Post(throttlingEnabled ? null : task);
            }
        }

        public virtual void Suspend()
        {
            lock (_taskQueue)
            {
                if (!_isSuspended)
                {
                    if (_dispatcher != null)
                    {
                        _dispatcher.QueueSuspendNotification();
                    }
                    _isSuspended = true;
                }
            }
        }

        public virtual void Resume()
        {
            lock (_taskQueue)
            {
                if (!_isSuspended)
                {
                    return;
                }
                if (_dispatcher != null)
                {
                    _dispatcher.QueueResumeNotification();
                }
                _isSuspended = false;
            }
            Enqueue(new Task(delegate
            {
            }));
        }

        public virtual bool TryDequeue(out ITask task)
        {
            if (_dispatcher == null)
            {
                throw new InvalidOperationException(Resource1.DispatcherPortTestNotValidInThreadpoolMode);
            }
            lock (_taskQueue)
            {
                task = null;
                if (_isDisposed)
                {
                    if ((_dispatcher.Options & DispatcherOptions.SuppressDisposeExceptions) == DispatcherOptions.None)
                    {
                        throw new ObjectDisposedException(typeof(DispatcherQueue).Name + ":" + Name);
                    }
                    bool result = false;
                    return result;
                }
                else
                {
                    if (_isSuspended)
                    {
                        bool result = false;
                        return result;
                    }
                    if (_taskCommonCount > 0)
                    {
                        task = TaskListRemoveFirst();
                        if (task == null)
                        {
                            _taskCommonCount = 0;
                        }
                    }
                    else if (_taskQueue.Count > 0)
                    {
                        task = _taskQueue.Dequeue();
                    }
                    if (task == null)
                    {
                        bool result = false;
                        return result;
                    }
                }
            }
            Interlocked.Decrement(ref _dispatcher._pendingTaskCount);
            return true;
        }

        private void RecalculateSchedulingRate()
        {
            _currentSchedulingRate = _scheduledItems / _watch.Elapsed.TotalSeconds;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isDisposed = true;
                if (_dispatcher == null)
                {
                    return;
                }
                if (_dispatcher.RemoveQueue(_name))
                {
                    lock (_taskQueue)
                    {
                        _dispatcher.AdjustPendingCount(-(_taskQueue.Count + _taskCommonCount));
                    }
                }
            }
        }

        internal bool RaiseUnhandledException(Exception exception)
        {
            if (_unhandledPort != null)
            {
                _unhandledPort.Post(exception);
            }
            if (UnhandledException != null)
            {
                UnhandledException(this, new UnhandledExceptionEventArgs(exception, false));
            }
            return _unhandledPort != null || UnhandledException != null;
        }
    }
}