using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Ccr.Core.Arbiters
{
    public abstract class ReceiverTask : TaskCommon
    {
        private ITask _task;

        internal ReceiverTaskState _state;

        internal IArbiterTask _arbiter;

        internal object _arbiterContext;

        public object ArbiterContext
        {
            get
            {
                return _arbiterContext;
            }
            set
            {
                _arbiterContext = value;
            }
        }

        public virtual IArbiterTask Arbiter
        {
            get
            {
                return _arbiter;
            }
            set
            {
                _arbiter = value;
            }
        }

        public ReceiverTaskState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public override IPortElement this[int index]
        {
            get
            {
                return _task[index];
            }
            set
            {
                _task[index] = value;
            }
        }

        public override int PortElementCount
        {
            get
            {
                if (_task == null)
                {
                    return 0;
                }
                return _task.PortElementCount;
            }
        }

        protected ITask UserTask
        {
            get
            {
                return _task;
            }
            set
            {
                _task = value;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}({1}) with {2} nested under \n    {3}", new object[]
            {
                base.GetType().Name,
                _state,
                (_task == null) ? "no continuation" : ("method " + _task.ToString()),
                (_arbiter == null) ? "none" : _arbiter.ToString()
            });
        }

        protected ReceiverTask()
        {
        }

        protected ReceiverTask(ITask taskToRun)
        {
            _task = taskToRun;
        }

        public override ITask PartialClone()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<ITask> Execute()
        {
            Arbiter = null;
            return null;
        }

        public abstract bool Evaluate(IPortElement messageNode, ref ITask deferredTask);

        public abstract void Consume(IPortElement item);

        public virtual void Cleanup()
        {
            _state = ReceiverTaskState.CleanedUp;
        }

        public abstract void Cleanup(ITask taskToCleanup);
    }
}