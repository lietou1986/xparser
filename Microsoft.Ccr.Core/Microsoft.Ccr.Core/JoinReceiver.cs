using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Ccr.Core
{
    public class JoinReceiver : JoinReceiverTask
    {
        private Receiver[] _ports;

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            Receiver[] ports = _ports;
            for (int i = 0; i < ports.Length; i++)
            {
                Receiver receiver = ports[i];
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}({1})] ", new object[]
                {
                    receiver._port.GetType().ToString(),
                    receiver._port.ItemCount.ToString(CultureInfo.InvariantCulture)
                });
            }
            return string.Format(CultureInfo.InvariantCulture, "\t{0}({1}) waiting on ports {2} with {3} nested under \n    {4}", new object[]
            {
                base.GetType().Name,
                _state,
                stringBuilder.ToString(),
                (base.UserTask == null) ? "no continuation" : ("method " + base.UserTask.ToString()),
                (_arbiter == null) ? "none" : _arbiter.ToString()
            });
        }

        internal JoinReceiver()
        {
        }

        public JoinReceiver(bool persist, ITask task, params IPortReceive[] ports) : base(task)
        {
            if (ports == null)
            {
                throw new ArgumentNullException("ports");
            }
            if (persist)
            {
                _state = ReceiverTaskState.Persistent;
            }
            if (ports == null || ports.Length == 0)
            {
                throw new ArgumentOutOfRangeException("aP", Resource1.JoinsMustHaveOnePortMinimumException);
            }
            _ports = new Receiver[ports.Length];
            int[] array = new int[ports.Length];
            int num = 0;
            for (int i = 0; i < ports.Length; i++)
            {
                IPortReceive portReceive = ports[i];
                int hashCode = portReceive.GetHashCode();
                Receiver receiver = new Receiver(portReceive);
                _ports[num] = receiver;
                array[num] = hashCode;
                receiver.ArbiterContext = num;
                num++;
            }
            Array.Sort<int, Receiver>(array, _ports);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Receiver[] ports = _ports;
            for (int i = 0; i < ports.Length; i++)
            {
                Receiver receiver = ports[i];
                receiver.Cleanup();
            }
        }

        public override void Cleanup(ITask taskToCleanup)
        {
            if (taskToCleanup == null)
            {
                throw new ArgumentNullException("taskToCleanup");
            }
            for (int i = 0; i < _ports.Length; i++)
            {
                Receiver receiver = _ports[i];
                ((IPortArbiterAccess)receiver._port).PostElement(taskToCleanup[(int)receiver.ArbiterContext]);
            }
        }

        protected override void Register()
        {
            Receiver[] ports = _ports;
            for (int i = 0; i < ports.Length; i++)
            {
                Receiver receiver = ports[i];
                if (_state == ReceiverTaskState.Persistent)
                {
                    receiver._state = ReceiverTaskState.Persistent;
                }
                receiver.Arbiter = this;
            }
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            deferredTask = null;
            return false;
        }

        public override void Consume(IPortElement item)
        {
        }

        protected override bool ShouldCommit()
        {
            if (_state == ReceiverTaskState.CleanedUp)
            {
                return false;
            }
            if (_arbiter == null || _arbiter.ArbiterState == ArbiterTaskState.Active)
            {
                Receiver[] ports = _ports;
                for (int i = 0; i < ports.Length; i++)
                {
                    Receiver receiver = ports[i];
                    if (receiver._port.ItemCount == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        protected override void Commit()
        {
            if (!ShouldCommit())
            {
                return;
            }
            ITask task = base.UserTask.PartialClone();
            IPortElement[] array = new IPortElement[_ports.Length];
            bool allTaken = true;
            for (int i = 0; i < _ports.Length; i++)
            {
                Receiver receiver = _ports[i];
                IPortElement portElement = ((IPortArbiterAccess)receiver._port).TestForElement();
                if (portElement == null)
                {
                    allTaken = false;
                    break;
                }
                array[i] = portElement;
                task[(int)receiver.ArbiterContext] = portElement;
            }
            base.Arbitrate(task, array, allTaken);
        }

        protected override void UnrollPartialCommit(IPortElement[] items)
        {
            for (int i = 0; i < _ports.Length; i++)
            {
                if (items[i] != null)
                {
                    ((IPortArbiterAccess)items[i].Owner).PostElement(items[i]);
                }
            }
        }
    }
}