using Microsoft.Ccr.Core.Arbiters;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public abstract class JoinReceiverTask : ReceiverTask, IArbiterTask, ITask
    {
        private int _commitAttempt;

        public override IArbiterTask Arbiter
        {
            get
            {
                return base.Arbiter;
            }
            set
            {
                base.Arbiter = value;
                if (base.TaskQueue == null)
                {
                    base.TaskQueue = base.Arbiter.TaskQueue;
                }
                Commit();
                if (_state == ReceiverTaskState.CleanedUp)
                {
                    return;
                }
                Register();
            }
        }

        public ArbiterTaskState ArbiterState
        {
            get
            {
                if (_arbiter != null)
                {
                    return _arbiter.ArbiterState;
                }
                if (base.State == ReceiverTaskState.CleanedUp)
                {
                    return ArbiterTaskState.Done;
                }
                return ArbiterTaskState.Active;
            }
        }

        internal JoinReceiverTask()
        {
        }

        internal JoinReceiverTask(ITask UserTask) : base(UserTask)
        {
        }

        protected abstract void Register();

        public abstract override void Cleanup(ITask taskToCleanup);

        protected abstract bool ShouldCommit();

        public bool Evaluate(ReceiverTask receiver, ref ITask deferredTask)
        {
            deferredTask = null;
            if (ShouldCommit())
            {
                deferredTask = new Task(new Handler(Commit));
            }
            return false;
        }

        protected abstract void Commit();

        protected void Arbitrate(ITask winner, IPortElement[] items, bool allTaken)
        {
            if (allTaken)
            {
                if (_state == ReceiverTaskState.Onetime && _arbiter == null)
                {
                    int num = Interlocked.Increment(ref _commitAttempt);
                    if (num > 1)
                    {
                        return;
                    }
                }
                ITask task = winner;
                if (_arbiter == null || _arbiter.Evaluate(this, ref task))
                {
                    if (_arbiter == null && task != null)
                    {
                        task.LinkedIterator = base.LinkedIterator;
                        task.ArbiterCleanupHandler = base.ArbiterCleanupHandler;
                    }
                    if (task != null)
                    {
                        base.TaskQueue.Enqueue(task);
                    }
                    if (_state == ReceiverTaskState.Onetime)
                    {
                        Cleanup();
                    }
                    return;
                }
            }
            base.TaskQueue.Enqueue(new Task<IPortElement[]>(items, new Handler<IPortElement[]>(UnrollPartialCommit)));
        }

        protected abstract void UnrollPartialCommit(IPortElement[] items);
    }
}