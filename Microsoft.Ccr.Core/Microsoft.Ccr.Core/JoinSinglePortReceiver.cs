using Microsoft.Ccr.Core.Arbiters;
using System;

namespace Microsoft.Ccr.Core
{
    public class JoinSinglePortReceiver : JoinReceiverTask
    {
        private IPortReceive _port;

        private int _count;

        internal JoinSinglePortReceiver()
        {
        }

        public JoinSinglePortReceiver(bool persist, ITask task, IPortReceive port, int count) : base(task)
        {
            if (persist)
            {
                _state = ReceiverTaskState.Persistent;
            }
            if (count <= 0)
            {
                throw new ArgumentException(Resource1.JoinSinglePortReceiverAtLeastOneItemMessage, "count");
            }
            _port = port;
            _count = count;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _port.UnregisterReceiver(this);
        }

        public override void Cleanup(ITask taskToCleanup)
        {
            if (taskToCleanup == null)
            {
                throw new ArgumentNullException("taskToCleanup");
            }
            for (int i = 0; i < _count; i++)
            {
                ((IPortArbiterAccess)_port).PostElement(taskToCleanup[i]);
            }
        }

        protected override void Register()
        {
            _port.RegisterReceiver(this);
        }

        protected override bool ShouldCommit()
        {
            if (_state != ReceiverTaskState.CleanedUp)
            {
                if (_arbiter != null && _arbiter.ArbiterState != ArbiterTaskState.Active)
                {
                    return false;
                }
                if (_port.ItemCount >= _count)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            deferredTask = null;
            if (ShouldCommit())
            {
                deferredTask = new Task(new Handler(Commit));
            }
            return false;
        }

        public override void Consume(IPortElement item)
        {
            if (ShouldCommit())
            {
                base.TaskQueue.Enqueue(new Task(new Handler(Commit)));
            }
        }

        protected override void Commit()
        {
            ITask task = base.UserTask.PartialClone();
            IPortElement[] array = ((IPortArbiterAccess)_port).TestForMultipleElements(_count);
            if (array != null)
            {
                for (int i = 0; i < _count; i++)
                {
                    task[i] = array[i];
                }
                base.Arbitrate(task, array, true);
            }
        }

        protected override void UnrollPartialCommit(IPortElement[] items)
        {
            IPortArbiterAccess portArbiterAccess = (IPortArbiterAccess)_port;
            for (int i = 0; i < _count; i++)
            {
                if (items[i] != null)
                {
                    portArbiterAccess.PostElement(items[i]);
                }
            }
        }
    }
}