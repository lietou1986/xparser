using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    internal class TaskExecutionWorker
    {
        private Dispatcher _dispatcher;

        internal Thread _thread;

        private AutoResetEvent _signal = new AutoResetEvent(false);

        private AutoResetEvent _proxySignal;

        private bool _isFastTimerLogicEnabled;

        public TaskExecutionWorker(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _isFastTimerLogicEnabled = ((dispatcher.Options & DispatcherOptions.UseHighAccuracyTimerLogic) > DispatcherOptions.None);
        }

        internal void Shutdown()
        {
            _thread = null;
            _signal.Set();
        }

        internal bool Signal()
        {
            AutoResetEvent autoResetEvent = Interlocked.Exchange<AutoResetEvent>(ref _proxySignal, null);
            if (autoResetEvent != null)
            {
                autoResetEvent.Set();
                return true;
            }
            return false;
        }

        private void WaitForTask(bool doTimedWait)
        {
            if (_dispatcher._pendingTaskCount > 0 && _dispatcher._cachedDispatcherQueueCount != _dispatcher._suspendedQueueCount)
            {
                return;
            }
            AutoResetEvent autoResetEvent = Interlocked.Exchange<AutoResetEvent>(ref _proxySignal, _signal);
            if (autoResetEvent != null)
            {
                if (doTimedWait)
                {
                    _signal.WaitOne(1, true);
                    return;
                }
                _signal.WaitOne();
            }
        }

        private void CheckStartupComplete()
        {
            int num = Interlocked.Increment(ref _dispatcher._workerCount);
            if (num == _dispatcher._taskExecutionWorkers.Count)
            {
                _dispatcher._startupCompleteEvent.Set();
            }
        }

        private void CheckShutdownComplete()
        {
            if (Interlocked.Decrement(ref _dispatcher._workerCount) == 0)
            {
                lock (_dispatcher._taskExecutionWorkers)
                {
                    _dispatcher._taskExecutionWorkers.Clear();
                }
            }
        }

        public static void ExecuteInCurrentThreadContext(object t)
        {
            ITask task = (ITask)t;
            try
            {
                TaskExecutionWorker.ExecuteTask(ref task, task.TaskQueue, false);
                Dispatcher.SetCurrentThreadCausalities(null);
            }
            catch (Exception ex)
            {
                if (TaskExecutionWorker.IsCriticalException(ex))
                {
                    throw;
                }
                TaskExecutionWorker.HandleException(task, ex);
            }
        }

        internal void ExecutionLoop()
        {
            CheckStartupComplete();
            while (true)
            {
                IL_06:
                int num = 0;
                int num2 = 0;
                ITask currentTask = null;
                try
                {
                    bool flag = false;
                    while (true)
                    {
                        if (num == 0)
                        {
                            if (_thread == null)
                            {
                                break;
                            }
                            WaitForTask(flag);
                        }
                        num2++;
                        num = 0;
                        int cachedDispatcherQueueCount = _dispatcher._cachedDispatcherQueueCount;
                        for (int i = 0; i < cachedDispatcherQueueCount; i++)
                        {
                            if (cachedDispatcherQueueCount != _dispatcher._cachedDispatcherQueueCount)
                            {
                                goto Block_5;
                            }
                            DispatcherQueue dispatcherQueue;
                            try
                            {
                                dispatcherQueue = _dispatcher._dispatcherQueues[(i + num2) % cachedDispatcherQueueCount];
                            }
                            catch
                            {
                                goto IL_06;
                            }
                            if (_isFastTimerLogicEnabled)
                            {
                                flag |= dispatcherQueue.CheckTimerExpirations();
                            }
                            if (dispatcherQueue.TryDequeue(out currentTask))
                            {
                                num += dispatcherQueue.Count;
                                TaskExecutionWorker.ExecuteTask(ref currentTask, dispatcherQueue, false);
                            }
                        }
                    }
                    Dispatcher.ClearCausalities();
                    CheckShutdownComplete();
                    Dispatcher.ClearCausalities();
                    break;
                    Block_5:
                    continue;
                }
                catch (Exception ex)
                {
                    if (TaskExecutionWorker.IsCriticalException(ex))
                    {
                        throw;
                    }
                    TaskExecutionWorker.HandleException(currentTask, ex);
                    continue;
                }
            }
        }

        private static bool IsCriticalException(Exception exception)
        {
            return exception is OutOfMemoryException || exception is SEHException;
        }

        private static void HandleException(ITask currentTask, Exception e)
        {
            Dispatcher.LogError(Resource1.HandleExceptionLog, e);
            Dispatcher.FilterExceptionThroughCausalities(currentTask, e);
            Dispatcher.SetCurrentThreadCausalities(null);
            if (currentTask != null && currentTask.ArbiterCleanupHandler != null)
            {
                try
                {
                    currentTask.ArbiterCleanupHandler();
                }
                catch (Exception exception)
                {
                    Dispatcher.LogError(Resource1.ExceptionDuringArbiterCleanup, exception);
                }
            }
        }

        private static void ExecuteTask(ref ITask currentTask, DispatcherQueue p, bool bypassExecute)
        {
            Handler arbiterCleanupHandler = currentTask.ArbiterCleanupHandler;
            IteratorContext iteratorContext;
            if (bypassExecute)
            {
                iteratorContext = null;
            }
            else
            {
                iteratorContext = TaskExecutionWorker.ExecuteTaskHelper(currentTask);
            }
            if (iteratorContext == null)
            {
                iteratorContext = (IteratorContext)currentTask.LinkedIterator;
            }
            if (iteratorContext != null)
            {
                Dispatcher.SetCurrentThreadCausalities(iteratorContext._causalities);
                TaskExecutionWorker.MoveIterator(ref currentTask, iteratorContext, ref arbiterCleanupHandler);
                if (currentTask != null)
                {
                    currentTask.LinkedIterator = iteratorContext;
                    currentTask.TaskQueue = p;
                    iteratorContext = TaskExecutionWorker.ExecuteTaskHelper(currentTask);
                    if (iteratorContext != null)
                    {
                        TaskExecutionWorker.NestIterator(currentTask);
                    }
                }
            }
            if (arbiterCleanupHandler != null)
            {
                if (currentTask != null)
                {
                    currentTask.ArbiterCleanupHandler = null;
                }
                arbiterCleanupHandler();
            }
        }

        private static void MoveIterator(ref ITask currentTask, IteratorContext iteratorContext, ref Handler finalizer)
        {
            lock (iteratorContext)
            {
                bool flag2 = false;
                try
                {
                    flag2 = !iteratorContext._iterator.MoveNext();
                    if (!flag2)
                    {
                        iteratorContext._causalities = Dispatcher.GetCurrentThreadCausalities();
                        currentTask = iteratorContext._iterator.Current;
                        currentTask.ArbiterCleanupHandler = finalizer;
                        finalizer = null;
                    }
                    else
                    {
                        if (currentTask != null)
                        {
                            finalizer = currentTask.ArbiterCleanupHandler;
                        }
                        else
                        {
                            finalizer = null;
                        }
                        currentTask = null;
                    }
                }
                catch (Exception)
                {
                    iteratorContext._iterator.Dispose();
                    throw;
                }
                finally
                {
                    if (flag2)
                    {
                        iteratorContext._iterator.Dispose();
                    }
                }
            }
        }

        private static void NestIterator(ITask currentTask)
        {
            ITask t = currentTask;
            Handler arbiterCleanup = t.ArbiterCleanupHandler;
            t.ArbiterCleanupHandler = delegate
            {
                t.ArbiterCleanupHandler = arbiterCleanup;
                TaskExecutionWorker.ExecuteTask(ref t, t.TaskQueue, true);
            };
            currentTask.TaskQueue.Enqueue(t);
        }

        private static IteratorContext ExecuteTaskHelper(ITask currentTask)
        {
            if (currentTask.LinkedIterator != null)
            {
                IteratorContext iteratorContext = (IteratorContext)currentTask.LinkedIterator;
                if (CausalityThreadContext.IsEmpty(iteratorContext._causalities))
                {
                    Dispatcher.ClearCausalities();
                }
                else
                {
                    Dispatcher.SetCurrentThreadCausalities(iteratorContext._causalities.Clone());
                }
            }
            else
            {
                Dispatcher.TransferCausalitiesFromTaskToCurrentThread(currentTask);
            }
            if (Debugger.IsAttached)
            {
                CausalityThreadContext currentThreadCausalities = Dispatcher.GetCurrentThreadCausalities();
                if (!CausalityThreadContext.IsEmpty(currentThreadCausalities) && CausalityThreadContext.RequiresDebugBreak(currentThreadCausalities))
                {
                    Debugger.Break();
                }
            }
            IEnumerator<ITask> enumerator = currentTask.Execute();
            if (enumerator != null)
            {
                return new IteratorContext(enumerator, Dispatcher.GetCurrentThreadCausalities());
            }
            return null;
        }
    }
}