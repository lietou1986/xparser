using System;

namespace Microsoft.Ccr.Core
{
    public static class DispatcherQueueExtensions
    {
        public static void Spawn(this DispatcherQueue TaskQueue, Handler handler)
        {
            TaskQueue.Enqueue(new Task(handler));
        }

        public static void Spawn<T0>(this DispatcherQueue TaskQueue, T0 t0, Handler<T0> handler)
        {
            TaskQueue.Enqueue(new Task<T0>(t0, handler));
        }

        public static void Spawn<T0, T1>(this DispatcherQueue TaskQueue, T0 t0, T1 t1, Handler<T0, T1> handler)
        {
            TaskQueue.Enqueue(new Task<T0, T1>(t0, t1, handler));
        }

        public static void Spawn<T0, T1, T2>(this DispatcherQueue TaskQueue, T0 t0, T1 t1, T2 t2, Handler<T0, T1, T2> handler)
        {
            TaskQueue.Enqueue(new Task<T0, T1, T2>(t0, t1, t2, handler));
        }

        public static void SpawnIterator(this DispatcherQueue TaskQueue, IteratorHandler handler)
        {
            TaskQueue.Enqueue(new IterativeTask(handler));
        }

        public static void SpawnIterator<T0>(this DispatcherQueue TaskQueue, T0 t0, IteratorHandler<T0> handler)
        {
            TaskQueue.Enqueue(new IterativeTask<T0>(t0, handler));
        }

        public static void SpawnIterator<T0, T1>(this DispatcherQueue TaskQueue, T0 t0, T1 t1, IteratorHandler<T0, T1> handler)
        {
            TaskQueue.Enqueue(new IterativeTask<T0, T1>(t0, t1, handler));
        }

        public static void SpawnIterator<T0, T1, T2>(this DispatcherQueue TaskQueue, T0 t0, T1 t1, T2 t2, IteratorHandler<T0, T1, T2> handler)
        {
            TaskQueue.Enqueue(new IterativeTask<T0, T1, T2>(t0, t1, t2, handler));
        }

        public static void Activate<T>(this DispatcherQueue TaskQueue, params T[] tasks) where T : ITask
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                ITask task = tasks[i];
                TaskQueue.Enqueue(task);
            }
        }

        public static void EmptyHandler<T>(this DispatcherQueue TaskQueue, T message)
        {
        }

        public static Port<DateTime> TimeoutPort(this DispatcherQueue TaskQueue, int milliseconds)
        {
            return TaskQueue.TimeoutPort(TimeSpan.FromMilliseconds((double)milliseconds));
        }

        public static Port<DateTime> TimeoutPort(this DispatcherQueue TaskQueue, TimeSpan ts)
        {
            Port<DateTime> port = new Port<DateTime>();
            TaskQueue.EnqueueTimer(ts, port);
            return port;
        }
    }
}