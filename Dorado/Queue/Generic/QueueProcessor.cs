using Dorado.Core;
using Microsoft.Ccr.Core;
using System;
using System.Text;
using System.Threading;
using ITask = Microsoft.Ccr.Core.ITask;

namespace Dorado.Queue.Generic
{
    public class QueueProcessor<T> : IDisposable, IReportable
    {
        public delegate void ProcessorDelegate(QueueProcessor<T> referenceQueue, T item);

        public delegate void CompletedDelegate();

        private string queueName;
        protected Dispatcher dispatcher;
        protected DispatcherQueue dispatcherQueue;
        protected Port<T> port;
        protected Port<EmptyValue> teardownPort;
        protected int running;
        protected int upperBound;
        protected long totalSecondsProcessing;
        private long totalItemsProcessed;
        protected long totalTimesProcessed;
        private long totalItemsQueued;
        protected long totalItemsLost;
        protected DateTime startTime;
        protected bool disposing;
        protected int activateItems;

        public ProcessorDelegate ProcessQueue;
        public CompletedDelegate CompletedQueue;

        public string QueueName
        {
            get
            {
                return this.queueName;
            }
        }

        public virtual int QueueLength
        {
            get
            {
                return this.dispatcher.PendingTaskCount + this.port.ItemCount;
            }
        }

        protected QueueProcessor()
        {
        }

        public QueueProcessor(string queueName)
            : this(queueName, 1, TaskExecutionPolicy.Unconstrained, ThreadPriority.Highest, 0, 0.0)
        {
        }

        public QueueProcessor(string queueName, int threadCount)
            : this(queueName, threadCount, TaskExecutionPolicy.Unconstrained, ThreadPriority.Highest, 0, 0.0)
        {
        }

        public QueueProcessor(string queueName, int threadCount, TaskExecutionQueuePolicy taskExecutionQueuePolicy, Priority priority, int upperBound, int activateItems)
        {
            this.queueName = queueName;
            this.activateItems = activateItems;
            if (threadCount > 0)
            {
                ThreadPriority threadPriority = this.ConvertThreadPriority(priority);
                this.dispatcher = DispatcherFactory.CreateDispatcher(threadCount, threadPriority, this.queueName);
            }
            else
            {
                this.dispatcher = DispatcherFactory.DefaultDispatcher;
            }
            if (upperBound > 0)
            {
                TaskExecutionPolicy taskExecutionPolicy = this.ConvertTaskExecutionPolicy(taskExecutionQueuePolicy);
                this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher, taskExecutionPolicy, upperBound);
            }
            else
            {
                this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher);
            }
            this.upperBound = upperBound;
            this.port = new Port<T>();
            this.teardownPort = new Port<EmptyValue>();
            ReportableObjectDirectory.Add(this.queueName, this);
        }

        public QueueProcessor(string queueName, int threadCount, TaskExecutionSchedulingPolicy taskExecutionSchedulingPolicy, Priority priority, double schedulingRate, int activateItems)
        {
            this.queueName = queueName;
            this.activateItems = activateItems;
            if (threadCount > 0)
            {
                ThreadPriority threadPriority = this.ConvertThreadPriority(priority);
                this.dispatcher = DispatcherFactory.CreateDispatcher(threadCount, threadPriority, this.queueName);
            }
            else
            {
                this.dispatcher = DispatcherFactory.DefaultDispatcher;
            }
            if (schedulingRate > 0.0)
            {
                TaskExecutionPolicy taskExecutionPolicy = this.ConvertTaskExecutionPolicy(taskExecutionSchedulingPolicy);
                this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher, taskExecutionPolicy, schedulingRate);
            }
            else
            {
                this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher);
            }
            this.upperBound = 0;
            this.port = new Port<T>();
            this.teardownPort = new Port<EmptyValue>();
            ReportableObjectDirectory.Add(this.queueName, this);
        }

        private ThreadPriority ConvertThreadPriority(Priority priority)
        {
            switch (priority)
            {
                case Priority.Lowest:
                    {
                        return ThreadPriority.Lowest;
                    }
                case Priority.BelowNormal:
                    {
                        return ThreadPriority.BelowNormal;
                    }
                case Priority.Normal:
                    {
                        return ThreadPriority.Normal;
                    }
                case Priority.AboveNormal:
                    {
                        return ThreadPriority.AboveNormal;
                    }
                case Priority.Highest:
                    {
                        return ThreadPriority.Highest;
                    }
                default:
                    {
                        return ThreadPriority.Normal;
                    }
            }
        }

        private TaskExecutionPolicy ConvertTaskExecutionPolicy(TaskExecutionQueuePolicy policy)
        {
            switch (policy)
            {
                case TaskExecutionQueuePolicy.ConstrainQueueDepthDiscardTasks:
                    {
                        return TaskExecutionPolicy.ConstrainQueueDepthDiscardTasks;
                    }
                case TaskExecutionQueuePolicy.ConstrainQueueDepthThrottleExecution:
                    {
                        return TaskExecutionPolicy.ConstrainQueueDepthThrottleExecution;
                    }
                default:
                    {
                        return TaskExecutionPolicy.Unconstrained;
                    }
            }
        }

        private TaskExecutionPolicy ConvertTaskExecutionPolicy(TaskExecutionSchedulingPolicy policy)
        {
            switch (policy)
            {
                case TaskExecutionSchedulingPolicy.ConstrainSchedulingRateDiscardTasks:
                    {
                        return TaskExecutionPolicy.ConstrainSchedulingRateDiscardTasks;
                    }
                case TaskExecutionSchedulingPolicy.ConstrainSchedulingRateThrottleExecution:
                    {
                        return TaskExecutionPolicy.ConstrainSchedulingRateThrottleExecution;
                    }
                default:
                    {
                        return TaskExecutionPolicy.Unconstrained;
                    }
            }
        }

        private QueueProcessor(string queueName, int threadCount, TaskExecutionPolicy taskExecutionPolicy, ThreadPriority threadPriority, int maximumQueueDepth, double schedulingRate)
        {
            this.queueName = queueName;
            if (threadCount > 0)
            {
                this.dispatcher = DispatcherFactory.CreateDispatcher(threadCount, this.queueName);
            }
            else
            {
                this.dispatcher = DispatcherFactory.DefaultDispatcher;
            }
            if (maximumQueueDepth > 0)
            {
                this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher, taskExecutionPolicy, maximumQueueDepth);
            }
            else
            {
                if (schedulingRate > 0.0)
                {
                    this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher, taskExecutionPolicy, schedulingRate);
                }
                else
                {
                    this.dispatcherQueue = new DispatcherQueue(this.queueName, this.dispatcher);
                }
            }
            this.upperBound = maximumQueueDepth;
            this.port = new Port<T>();
            this.teardownPort = new Port<EmptyValue>();
            ReportableObjectDirectory.Add(this.queueName, this);
        }

        public virtual bool Enqueue(T item)
        {
            bool allowOnQueue = this.dispatcherQueue.Policy != TaskExecutionPolicy.ConstrainQueueDepthDiscardTasks || this.upperBound <= 0 || this.dispatcher.PendingTaskCount + this.port.ItemCount + this.dispatcher.WorkerThreadCount < this.upperBound;
            if (allowOnQueue)
            {
                LoggerWrapper.Logger.Debug(string.Format("Item enqueued to queue {0} of type {1}", new object[]
                    {
                        this.queueName,
                        typeof(T).FullName
                    }));

                this.port.Post(item);
                Interlocked.Increment(ref this.totalItemsQueued);
            }
            else
            {
                LoggerWrapper.Logger.Debug(string.Format("Upper bound of {0} hit on queue {1}", new object[]
                    {
                        this.upperBound,
                        this.queueName
                    }));

                Interlocked.Increment(ref this.totalItemsLost);
            }
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 0)
            {
                LoggerWrapper.Logger.Debug(string.Format("Queue {0} started", new object[]
                    {
                        this.queueName
                    }));

                this.startTime = DateTime.Now;
                if (this.activateItems > 0)
                {
                    this.ActivateProcessQueue();
                }
                else
                {
                    this.InternalProcessQueue();
                }
            }
            return allowOnQueue;
        }

        protected virtual void InternalProcessQueue()
        {
            Receiver<T> receiver = Arbiter.Receive<T>(true, this.port, delegate (T item)
            {
                this.QueueHandler(item);
            }
            );
            Arbiter.Activate(this.dispatcherQueue, new ITask[]
            {
                receiver
            });
        }

        protected virtual void ActivateProcessQueue()
        {
            Arbiter.Activate(this.dispatcherQueue, new ITask[]
            {
                Arbiter.MultipleItemReceive<T>(true, this.port, this.activateItems, delegate(T[] items)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        T item = items[i];
                        this.QueueHandler(item);
                    }
                }
                )
            });
        }

        private void QueueHandler(T item)
        {
            DateTime processStart = DateTime.Now;
            try
            {
                LoggerWrapper.Logger.Debug(string.Format("Processing item of type {0} from queue {1}", new object[]
                    {
                        typeof(T).FullName,
                        this.queueName
                    }));

                if (item is IQueueProcessable)
                {
                    ((IQueueProcessable)item).ProcessItem(item);
                }
                else
                {
                    this.ProcessQueue(this, item);
                }
                Interlocked.Increment(ref this.totalItemsProcessed);
                Interlocked.Increment(ref this.totalTimesProcessed);
                if (this.totalItemsProcessed == this.totalItemsQueued && this.CompletedQueue != null)
                {
                    this.CompletedQueue();
                }
            }
            catch (Exception arg_B6_0)
            {
                Exception ex = arg_B6_0;
                LoggerWrapper.Logger.Error(string.Format("Error processing queue {0} - Exception {1} : {2}", new object[]
                {
                    this.queueName,
                    ex.Message,
                    ex.StackTrace
                }));
            }
            Interlocked.Add(ref this.totalSecondsProcessing, (long)Math.Round(TimeSpan.FromTicks(DateTime.Now.Ticks - processStart.Ticks).TotalSeconds));
        }

        public virtual string CreateReport()
        {
            StringBuilder report = new StringBuilder();
            report.Append("<div>\n");
            report.AppendFormat("<b>Queue Name:</b> {0}<br />\n", this.queueName);
            report.AppendFormat("<b>Queue TypeName:</b> {0}<br />\n", typeof(T).FullName);
            try
            {
                report.AppendFormat("<b>Queue'd Items:</b> {0}<br />\n", this.port.ItemCount);
                report.AppendFormat("<b>Items Being Processed:</b> {0}<br />\n", this.dispatcherQueue.Count);
                report.AppendFormat("<b>Thread Pool Type:</b> {0}<br />\n", (this.dispatcher.Name == DispatcherFactory.DefaultDispatcher.Name) ? "Shared" : "Individual");
                report.AppendFormat("<b>Thread Count:</b> {0}<br />\n", this.dispatcher.WorkerThreadCount);
                report.AppendFormat("<b>Total Items Enqueued:</b> {0}<br />\n", this.totalItemsQueued);
                report.AppendFormat("<b>Total Items Processed:</b> {0}<br />\n", this.totalItemsProcessed);
                if (this.upperBound > 0)
                {
                    report.AppendFormat("<b>Total Items Lost (due to upperbound hits)</b>: {0}<br />\n", this.totalItemsLost);
                }
                report.AppendFormat("<b>Total Times Processed:</b> {0}<br />\n", this.totalTimesProcessed);
                report.AppendFormat("<b>Total Seconds Processing (approx):<b> {0}<br />\n", this.totalSecondsProcessing);
                double totalMinutes = TimeSpan.FromTicks(DateTime.Now.Ticks - this.startTime.Ticks).TotalMinutes;
                double averageItemsQueuedAMinute = (double)this.totalItemsQueued / totalMinutes;
                double averageItemsProcessedAMinute = (double)this.totalItemsProcessed / totalMinutes;
                double averageProcessingTime = (double)this.totalTimesProcessed / (double)this.totalTimesProcessed;
                report.AppendFormat("<b>Total Running Time:</b> {0}<br />\n", totalMinutes);
                report.AppendFormat("<b>Average Items Queued a Minute:</b> {0}<br />\n", averageItemsQueuedAMinute);
                report.AppendFormat("<b>Average Items Processed a Minute:</b> {0}<br />\n", averageItemsProcessedAMinute);
                report.AppendFormat("<b>Average Processing Time (approx, in seconds):</b> {0}<br />\n", averageProcessingTime);
            }
            catch (Exception ex)
            {
                report.Append(ex.Message + " " + ex.StackTrace);
            }
            report.Append("<br />\n");
            report.Append("</div>\n");
            return report.ToString();
        }

        public virtual void Dispose()
        {
            this.disposing = true;
            ReportableObjectDirectory.Remove(this.queueName);
            if (this.dispatcher.Name != DispatcherFactory.DefaultDispatcher.Name)
            {
                this.dispatcherQueue.Dispose();
                this.dispatcher.Dispose();
            }
            else
            {
                this.dispatcherQueue.Dispose();
                if (DispatcherFactory.DefaultDispatcher != null && DispatcherFactory.DefaultDispatcher.DispatcherQueues.Count == 0)
                {
                    this.dispatcher = null;
                    DispatcherFactory.DestroyDefaultDispatcher();
                }
            }
            GC.SuppressFinalize(this);
            LoggerWrapper.Logger.Debug(string.Format("Teardown of queue {0} completed", new object[]
                {
                    this.queueName
                }));
        }
    }
}