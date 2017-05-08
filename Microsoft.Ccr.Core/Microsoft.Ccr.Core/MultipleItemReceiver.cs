using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public class MultipleItemReceiver : ReceiverTask
    {
        private ITask _userTask;

        private IPortReceive[] _ports;

        private Receiver[] _receivers;

        private int _pendingItemCount;

        public override IArbiterTask Arbiter
        {
            set
            {
                base.Arbiter = value;
                if (base.TaskQueue == null)
                {
                    base.TaskQueue = base.Arbiter.TaskQueue;
                }
                Register();
            }
        }

        public MultipleItemReceiver(ITask userTask, params IPortReceive[] ports)
        {
            if (ports == null)
            {
                throw new ArgumentNullException("ports");
            }
            if (userTask == null)
            {
                throw new ArgumentNullException("userTask");
            }
            if (ports.Length == 0)
            {
                throw new ArgumentOutOfRangeException("ports");
            }
            _ports = ports;
            _userTask = userTask;
            _pendingItemCount = ports.Length;
            _receivers = new Receiver[_ports.Length];
        }

        public new ITask PartialClone()
        {
            return new MultipleItemReceiver(_userTask.PartialClone(), _ports);
        }

        public override IEnumerator<ITask> Execute()
        {
            base.Execute();
            return null;
        }

        private void Register()
        {
            int num = 0;
            IPortReceive[] ports = _ports;
            for (int i = 0; i < ports.Length; i++)
            {
                IPortReceive port = ports[i];
                Receiver receiver = new MultipleItemHelperReceiver(port, this);
                receiver._arbiterContext = num;
                _receivers[num++] = receiver;
                receiver.TaskQueue = base.TaskQueue;
            }
            num = 0;
            IPortReceive[] ports2 = _ports;
            for (int j = 0; j < ports2.Length; j++)
            {
                IPortReceive portReceive = ports2[j];
                portReceive.RegisterReceiver(_receivers[num++]);
            }
        }

        internal bool Evaluate(int index, IPortElement item, ref ITask deferredTask)
        {
            if (base.State == ReceiverTaskState.CleanedUp)
            {
                return false;
            }
            if (_userTask[index] != null)
            {
                throw new InvalidOperationException();
            }
            _userTask[index] = item;
            int num = Interlocked.Decrement(ref _pendingItemCount);
            if (num > 0)
            {
                return true;
            }
            if (num == 0)
            {
                _userTask.LinkedIterator = base.LinkedIterator;
                _userTask.TaskQueue = base.TaskQueue;
                _userTask.ArbiterCleanupHandler = base.ArbiterCleanupHandler;
                deferredTask = _userTask;
                if (Arbiter != null)
                {
                    if (!Arbiter.Evaluate(this, ref deferredTask))
                    {
                        return false;
                    }
                    _userTask = null;
                }
                return true;
            }
            return false;
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            throw new NotImplementedException();
        }

        public override void Consume(IPortElement item)
        {
            throw new NotImplementedException();
        }

        public override void Cleanup()
        {
            base.State = ReceiverTaskState.CleanedUp;
            Receiver[] receivers = _receivers;
            for (int i = 0; i < receivers.Length; i++)
            {
                Receiver receiver = receivers[i];
                if (receiver != null)
                {
                    receiver._port.UnregisterReceiver(receiver);
                }
            }
            if (_userTask != null)
            {
                Cleanup(_userTask);
            }
        }

        public override void Cleanup(ITask taskToCleanup)
        {
            for (int i = 0; i < _ports.Length; i++)
            {
                IPortElement portElement = taskToCleanup[i];
                if (portElement != null)
                {
                    ((IPort)_ports[i]).TryPostUnknownType(taskToCleanup[i].Item);
                }
            }
        }
    }
}