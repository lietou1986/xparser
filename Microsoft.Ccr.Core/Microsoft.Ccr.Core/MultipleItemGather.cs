using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public class MultipleItemGather : ReceiverTask, ITask
    {
        private Handler<ICollection[]> _handler;

        private Type[] _types;

        private IPortReceive[] _ports;

        private Dictionary<Type, List<object>> _lookupTable = new Dictionary<Type, List<object>>();

        private Receiver[] _receivers;

        private int _expectedItemCount;

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

        public MultipleItemGather(Type[] types, IPortReceive[] ports, int itemCount, Handler<ICollection[]> handler)
        {
            _state = ReceiverTaskState.Onetime;
            if (ports == null)
            {
                throw new ArgumentNullException("ports");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            if (ports.Length == 0)
            {
                throw new ArgumentOutOfRangeException("ports");
            }
            if (types.Length == 0)
            {
                throw new ArgumentOutOfRangeException("types");
            }
            if (types.Length != ports.Length)
            {
                throw new ArgumentOutOfRangeException("types", "Type array length must match port array length");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _types = types;
            _ports = ports;
            _handler = handler;
            _pendingItemCount = itemCount;
            _expectedItemCount = itemCount;
            Type[] types2 = _types;
            for (int i = 0; i < types2.Length; i++)
            {
                Type key = types2[i];
                _lookupTable.Add(key, new List<object>());
            }
        }

        public new ITask PartialClone()
        {
            return new MultipleItemGather(_types, _ports, _pendingItemCount, _handler);
        }

        public override IEnumerator<ITask> Execute()
        {
            base.Execute();
            Register();
            return null;
        }

        private void Register()
        {
            int num = 0;
            _receivers = new Receiver[_ports.Length];
            IPortReceive[] ports = _ports;
            for (int i = 0; i < ports.Length; i++)
            {
                IPortReceive portReceive = ports[i];
                Receiver receiver = new GatherPrivateReceiver(portReceive, this);
                _receivers[num++] = receiver;
                receiver.TaskQueue = base.TaskQueue;
                portReceive.RegisterReceiver(receiver);
                if (_pendingItemCount <= 0)
                {
                    return;
                }
            }
        }

        internal bool Evaluate(object item, ref ITask deferredTask)
        {
            int num = Interlocked.Decrement(ref _pendingItemCount);
            if (num < 0 || _state == ReceiverTaskState.CleanedUp)
            {
                return false;
            }
            Type type = item.GetType();
            List<object> list;
            _lookupTable.TryGetValue(type, out list);
            if (list == null)
            {
                Type baseType = type.BaseType;
                while (baseType != null)
                {
                    _lookupTable.TryGetValue(baseType, out list);
                    if (list != null)
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
                if (list == null)
                {
                    Type[] interfaces = type.GetInterfaces();
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        Type key = interfaces[i];
                        _lookupTable.TryGetValue(key, out list);
                        if (list != null)
                        {
                            break;
                        }
                    }
                    if (list == null)
                    {
                        throw new InvalidOperationException("No result collection found for type:" + type);
                    }
                }
            }
            lock (list)
            {
                list.Add(item);
            }
            if (num != 0)
            {
                return true;
            }
            ICollection[] array = new ICollection[_ports.Length];
            int num2 = 0;
            Type[] types = _types;
            for (int j = 0; j < types.Length; j++)
            {
                Type key2 = types[j];
                array[num2++] = _lookupTable[key2];
            }
            deferredTask = new Task<ICollection[]>(array, _handler)
            {
                LinkedIterator = base.LinkedIterator,
                TaskQueue = base.TaskQueue,
                ArbiterCleanupHandler = base.ArbiterCleanupHandler
            };
            if (Arbiter == null)
            {
                _lookupTable.Clear();
                Cleanup();
                return true;
            }
            if (!Arbiter.Evaluate(this, ref deferredTask))
            {
                return false;
            }
            _lookupTable.Clear();
            return true;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Receiver[] receivers = _receivers;
            for (int i = 0; i < receivers.Length; i++)
            {
                Receiver receiver = receivers[i];
                if (receiver != null)
                {
                    receiver._port.UnregisterReceiver(receiver);
                }
            }
            if (_lookupTable.Count > 0)
            {
                ICollection[] array = new ICollection[_ports.Length];
                int num = 0;
                Type[] types = _types;
                for (int j = 0; j < types.Length; j++)
                {
                    Type key = types[j];
                    array[num++] = _lookupTable[key];
                }
                UnrollPartialCommit(array);
            }
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            throw new InvalidOperationException();
        }

        public override void Consume(IPortElement item)
        {
            throw new InvalidOperationException();
        }

        public override void Cleanup(ITask taskToCleanup)
        {
            ICollection[] results = (ICollection[])taskToCleanup[0].Item;
            UnrollPartialCommit(results);
        }

        private void UnrollPartialCommit(ICollection[] results)
        {
            for (int i = 0; i < results.Length; i++)
            {
                ICollection collection = results[i];
                IPort port = null;
                if (collection != null)
                {
                    foreach (object current in collection)
                    {
                        if (port == null)
                        {
                            IPortReceive[] ports = _ports;
                            for (int j = 0; j < ports.Length; j++)
                            {
                                IPort port2 = (IPort)ports[j];
                                if (port2.TryPostUnknownType(current))
                                {
                                    port = port2;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            port.PostUnknownType(current);
                        }
                    }
                }
            }
        }
    }
}