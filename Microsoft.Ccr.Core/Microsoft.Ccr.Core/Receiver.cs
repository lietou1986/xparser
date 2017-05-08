using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Diagnostics;

namespace Microsoft.Ccr.Core
{
    public class Receiver : ReceiverTask
    {
        internal IPortReceive _port;

        private bool _keepItemInPort;

        internal bool KeepItemInPort
        {
            get
            {
                return _keepItemInPort;
            }
            set
            {
                _keepItemInPort = value;
            }
        }

        public override IArbiterTask Arbiter
        {
            set
            {
                base.Arbiter = value;
                if (base.TaskQueue == null)
                {
                    base.TaskQueue = base.Arbiter.TaskQueue;
                }
                _port.RegisterReceiver(this);
            }
        }

        internal Receiver()
        {
        }

        internal Receiver(IPortReceive port)
        {
            _port = port;
        }

        public Receiver(IPortReceive port, ITask task) : this(false, port, task)
        {
        }

        public Receiver(bool persist, IPortReceive port, ITask task) : base(task)
        {
            if (persist)
            {
                _state = ReceiverTaskState.Persistent;
            }
            _port = port;
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
            ((IPortArbiterAccess)_port).PostElement(taskToCleanup[0]);
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            if (_state == ReceiverTaskState.CleanedUp)
            {
                return false;
            }
            if (base.UserTask != null)
            {
                if (_state == ReceiverTaskState.Persistent)
                {
                    deferredTask = base.UserTask.PartialClone();
                }
                else
                {
                    deferredTask = base.UserTask;
                }
                deferredTask[0] = messageNode;
            }
            else if (_keepItemInPort)
            {
                Cleanup();
                deferredTask = new Task(delegate
                {
                });
            }
            if (_arbiter != null)
            {
                bool flag = _arbiter.Evaluate(this, ref deferredTask);
                return !_keepItemInPort && flag;
            }
            if (deferredTask != null)
            {
                deferredTask.LinkedIterator = base.LinkedIterator;
                deferredTask.ArbiterCleanupHandler = base.ArbiterCleanupHandler;
            }
            return !_keepItemInPort;
        }

        public override void Consume(IPortElement item)
        {
            if (_state == ReceiverTaskState.CleanedUp)
            {
                return;
            }
            ITask task = base.UserTask.PartialClone();
            task[0] = item;
            task.LinkedIterator = base.LinkedIterator;
            task.ArbiterCleanupHandler = base.ArbiterCleanupHandler;
            base.TaskQueue.Enqueue(task);
        }
    }

    public class Receiver<T> : Receiver
    {
        private Predicate<T> _predicate;

        public Predicate<T> Predicate
        {
            get
            {
                return _predicate;
            }
            set
            {
                _predicate = value;
            }
        }

        internal Receiver()
        {
        }

        internal Receiver(IPortReceive port) : base(port)
        {
        }

        public Receiver(IPortReceive port, Predicate<T> predicate, Task<T> task) : this(false, port, predicate, task)
        {
        }

        public Receiver(bool persist, IPortReceive port, Predicate<T> predicate, Task<T> task) : base(persist, port, task)
        {
            _predicate = predicate;
        }

        public Receiver(bool persist, IPortReceive port, Predicate<T> predicate, IterativeTask<T> task) : base(persist, port, task)
        {
            _predicate = predicate;
        }

        public Receiver(IPortReceive port, Predicate<T> predicate, IterativeTask<T> task) : base(port, task)
        {
            _predicate = predicate;
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            if (_predicate == null)
            {
                return base.Evaluate(messageNode, ref deferredTask);
            }
            bool result;
            try
            {
                if (_predicate((T)((object)messageNode.Item)))
                {
                    result = base.Evaluate(messageNode, ref deferredTask);
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception arg)
            {
                if (Dispatcher.TraceSwitchCore.TraceError)
                {
                    Trace.WriteLine("Predicate caused an exception, ignoring message. Exception:" + arg);
                }
                result = false;
            }
            return result;
        }
    }
}