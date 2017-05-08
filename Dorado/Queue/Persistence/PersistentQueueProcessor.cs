using Dorado.Core;
using Dorado.Utils;
using Microsoft.Ccr.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ITask = Microsoft.Ccr.Core.ITask;

namespace Dorado.Queue.Persistence
{
    public class PersistentQueueProcessor<T> where T : class, new()
    {
        public delegate bool QueueItemHandler(PersistentQueueProcessor<T> queue, T item);

        private string queueName;
        private QueuePersistence<T> persistence;
        private int dequeueInterval;
        private int dequeueBatch;
        private Timer dequeueTimer;
        private readonly object queueLock = new object();
        private Dispatcher dispatcher;
        private DispatcherQueue queue;
        private Port<PersistentQueueItem<T>> port = new Port<PersistentQueueItem<T>>();
        private QueueItemHandler queueItemHandler;
        private long isRunning;

        private int maxTry = 10;
        private int failPenalty = 5000;
        private static readonly Regex ValidQueueNameRegex = new Regex("^[a-zA-Z0-9.\\-_]+$");

        private bool IsRunning
        {
            get
            {
                return Interlocked.Read(ref this.isRunning) == 1L;
            }
            set
            {
                Interlocked.Exchange(ref this.isRunning, value ? 1L : 0L);
            }
        }

        public int MaxTry
        {
            get
            {
                return this.maxTry;
            }
            set
            {
                Guard.ArgumentPositive(value);
                this.maxTry = value;
            }
        }

        public int FailPenalty
        {
            get
            {
                return this.failPenalty;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "FailPenalty must >= 0");
                }
                this.failPenalty = value;
            }
        }

        private long FailPenaltyTicks
        {
            get
            {
                return TimeSpan.FromMilliseconds((double)this.failPenalty).Ticks;
            }
        }

        public int Length
        {
            get
            {
                return this.persistence.Count;
            }
        }

        public PersistentQueueProcessor(string queueName, QueueItemHandler queueItemHandler)
        {
            this.Init(queueName, queueItemHandler, 0, ThreadPriority.Normal, 1000, 10, 100, GetQueuePersistPath(queueName));
        }

        public PersistentQueueProcessor(string queueName, QueueItemHandler queueItemHandler, int threadCount, ThreadPriority threadPriority)
        {
            this.Init(queueName, queueItemHandler, threadCount, threadPriority, 1000, 10, 100, GetQueuePersistPath(queueName));
        }

        public PersistentQueueProcessor(string queueName, QueueItemHandler queueItemHandler, int scheduleRate)
        {
            this.Init(queueName, queueItemHandler, 0, ThreadPriority.Normal, 1000, scheduleRate, GetQueuePersistPath(queueName));
        }

        public PersistentQueueProcessor(string queueName, QueueItemHandler queueItemHandler, int threadCount, ThreadPriority threadPriority, int scheduleRate)
        {
            this.Init(queueName, queueItemHandler, threadCount, threadPriority, 1000, scheduleRate, GetQueuePersistPath(queueName));
        }

        private void Init(string queueName, QueueItemHandler queueItemHandler, int threadCount, ThreadPriority threadPriority, int maxItemsInMemory, int scheduleRate, string persistPath)
        {
            Guard.ArgumentNotNull<QueueItemHandler>(queueItemHandler);
            Guard.ArgumentValuesPositive(new int[]
            {
                maxItemsInMemory,
                scheduleRate
            });
            int interval = Math.Max(1, 1000 / scheduleRate);
            Math.Max(1, scheduleRate / (1000 / interval));
            this.InitQueue(queueName, queueItemHandler, threadCount, threadPriority, maxItemsInMemory, interval, Math.Max(1, scheduleRate / 1000), persistPath);
        }

        private void Init(string queueName, QueueItemHandler queueItemHandler, int threadCount, ThreadPriority threadPriority, int maxItemsInMemory, int dequeueInterval, int dequeueBatch, string persistPath)
        {
            Guard.ArgumentNotNull<QueueItemHandler>(queueItemHandler);
            Guard.ArgumentValuesPositive(new int[]
            {
                maxItemsInMemory,
                dequeueInterval,
                dequeueBatch
            });
            this.InitQueue(queueName, queueItemHandler, threadCount, threadPriority, maxItemsInMemory, dequeueInterval, dequeueBatch, persistPath);
        }

        private void InitQueue(string queueName, QueueItemHandler queueItemHandler, int threadCount, ThreadPriority threadPriority, int maxItemsInMemory, int dequeueInterval, int dequeueBatch, string persistPath)
        {
            this.queueName = queueName;
            this.queueItemHandler = queueItemHandler;
            this.persistence = new EfzQueuePersistence<T>(persistPath);
            this.dequeueInterval = dequeueInterval;
            this.dequeueBatch = dequeueBatch;
            this.dispatcher = new Dispatcher(threadCount, threadPriority, true, "Thread Pool - " + queueName);
            this.queue = new DispatcherQueue(queueName, this.dispatcher, TaskExecutionPolicy.ConstrainQueueDepthThrottleExecution, maxItemsInMemory);
            Arbiter.Activate(this.queue, new ITask[]
            {
                Arbiter.Receive<PersistentQueueItem<T>>(true, this.port, new Handler<PersistentQueueItem<T>>(this.InternalQueueItemHandler))
            });
            this.dequeueTimer = new Timer(new TimerCallback(this.Dequeue), null, -1, -1);
        }

        public void Enqueue(T item)
        {
            Guard.ArgumentNotNull<T>(item);
            try
            {
                Monitor.Enter(this.queueLock);
                this.persistence.Save(new PersistentQueueItem<T>(item));
                Monitor.PulseAll(this.queueLock);
            }
            finally
            {
                Monitor.Exit(this.queueLock);
            }
        }

        private void Dequeue(object state)
        {
            if (this.IsRunning)
            {
                List<PersistentQueueItem<T>> items = null;
                try
                {
                    Monitor.Enter(this.queueLock);
                    while (this.Length == 0)
                    {
                        Monitor.Wait(this.queueLock);
                    }
                    if (this.IsRunning)
                    {
                        items = this.persistence.MultiLoad(this.dequeueBatch);
                    }
                }
                catch (Exception ex)
                {
                    LoggerWrapper.Logger.Error("PersistentQueueProcessor", ex);
                }
                finally
                {
                    Monitor.Exit(this.queueLock);
                }
                if (items != null && items.Count > 0)
                {
                    foreach (PersistentQueueItem<T> item in items)
                    {
                        this.port.Post(item);
                    }
                }
                if (this.IsRunning)
                {
                    this.dequeueTimer.Change(this.dequeueInterval, -1);
                }
            }
        }

        public void Purge()
        {
            this.persistence.Purge();
        }

        public void Stop()
        {
            this.IsRunning = false;
            this.dequeueTimer.Change(-1, -1);
        }

        public void Start()
        {
            this.IsRunning = true;
            this.dequeueTimer.Change(this.dequeueInterval, -1);
        }

        private void InternalQueueItemHandler(PersistentQueueItem<T> item)
        {
            try
            {
                bool success = this.queueItemHandler(this, item.Payload);
                if (success)
                {
                    this.persistence.Remove(item);
                }
                else
                {
                    item.Try++;
                    if (item.Try < this.maxTry)
                    {
                        item.Priority += this.FailPenaltyTicks;
                        this.persistence.Fail(item);
                        Monitor.Enter(this.queueLock);
                        Monitor.PulseAll(this.queueLock);
                        Monitor.Exit(this.queueLock);
                    }
                    else
                    {
                        this.persistence.Discard(item);
                        LoggerWrapper.Logger.Error(string.Format("{0}: Discard item after {1} try: EnqueueTime: {2}, Data: {3}", new object[]
                        {
                            this.queueName,
                            this.maxTry,
                            item.EnqueueTime,
                            item.PayloadToJson()
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("PersistentQueueProcessor", ex);
            }
        }

        private static string GetQueuePersistPath(string queueName)
        {
            CheckQueueName(queueName);
            StackFrame frame = new StackFrame(2);
            string declaringClass = frame.GetMethod().DeclaringType.FullName;
            string relativePath = string.Format("{0}\\{1}\\{2}\\{2}", ConfigUtility.ApplicationName, declaringClass, queueName);
            return Path.Combine(PersistentQueueConfig.PersistenceRootPath, relativePath);
        }

        private static void CheckQueueName(string queueName)
        {
            Guard.ArgumentNotEmpty(queueName);
            if (!ValidQueueNameRegex.IsMatch(queueName))
            {
                throw new QueueException("Invalid queue name, only chars in [a-zA-Z0-9.-_] is allowed");
            }
        }
    }
}