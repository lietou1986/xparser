using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public class PortSet<T0, T1> : IPortSet, IPort
    {
        private Port<T0> _p0;

        private Port<T1> _p1;

        private Port<object> _sharedPort;

        private PortSetMode _mode;

        private Type[] _types;

        public Port<T0> P0
        {
            get
            {
                if (Mode == PortSetMode.SharedPort)
                {
                    throw new InvalidOperationException();
                }
                if (_p0 == null)
                {
                    lock (this)
                    {
                        if (_p0 == null)
                        {
                            _p0 = new Port<T0>();
                        }
                    }
                }
                return _p0;
            }
        }

        public Port<T1> P1
        {
            get
            {
                if (Mode == PortSetMode.SharedPort)
                {
                    throw new InvalidOperationException();
                }
                if (_p1 == null)
                {
                    lock (this)
                    {
                        if (_p1 == null)
                        {
                            _p1 = new Port<T1>();
                        }
                    }
                }
                return _p1;
            }
        }

        public ICollection<IPort> Ports
        {
            get
            {
                return new IPort[]
                {
                    P0,
                    P1
                };
            }
        }

        public IPort this[Type portItemType]
        {
            get
            {
                if (typeof(T0).IsAssignableFrom(portItemType))
                {
                    return P0;
                }
                if (typeof(T1).IsAssignableFrom(portItemType))
                {
                    return P1;
                }
                return null;
            }
        }

        public Port<object> SharedPort
        {
            get
            {
                return _sharedPort;
            }
        }

        public PortSetMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (value == PortSetMode.SharedPort && _sharedPort == null)
                {
                    _sharedPort = new Port<object>();
                    return;
                }
                if (value == PortSetMode.Default && _sharedPort != null)
                {
                    _sharedPort = null;
                }
            }
        }

        public PortSet()
        {
        }

        public PortSet(PortSetMode mode) : this()
        {
            Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1)
        {
            _p0 = parameter0;
            _p1 = parameter1;
        }

        public void Post(T0 item)
        {
            if (_mode == PortSetMode.SharedPort)
            {
                _sharedPort.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (_mode == PortSetMode.SharedPort)
            {
                _sharedPort.Post(item);
                return;
            }
            P1.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1> port)
        {
            return port.P1;
        }

        public static implicit operator T0(PortSet<T0, T1> port)
        {
            return port.Test<T0>();
        }

        public static implicit operator T1(PortSet<T0, T1> port)
        {
            return port.Test<T1>();
        }

        public T Test<T>()
        {
            if (_mode == PortSetMode.SharedPort)
            {
                return (T)((object)_sharedPort.Test());
            }
            IPortReceive portReceive = this[typeof(T)] as IPortReceive;
            if (portReceive == null)
            {
                throw new PortNotFoundException();
            }
            return (T)((object)portReceive.Test());
        }

        public static implicit operator Choice(PortSet<T0, T1> portSet)
        {
            return PortSet.ImplicitChoiceOperator(portSet);
        }

        public void PostUnknownType(object item)
        {
            if (!TryPostUnknownType(item))
            {
                throw new PortNotFoundException();
            }
        }

        public bool TryPostUnknownType(object item)
        {
            if (_mode == PortSetMode.SharedPort)
            {
                _sharedPort.Post(item);
                return true;
            }
            Type portItemType = null;
            if (item == null)
            {
                if (_types == null)
                {
                    _types = new Type[]
                    {
                        typeof(T0),
                        typeof(T1)
                    };
                }
                if (!PortSet.FindTypeFromRuntimeType(item, _types, out portItemType))
                {
                    return false;
                }
            }
            else
            {
                portItemType = item.GetType();
            }
            IPort port = this[portItemType];
            return port != null && port.TryPostUnknownType(item);
        }
    }

    public class PortSet : IPortSet, IPort
    {
        protected Port<object> SharedPortInternal;

        protected IPort[] PortsTable;

        protected Type[] Types;

        protected PortSetMode ModeInternal;

        public ICollection<IPort> Ports
        {
            get
            {
                if (ModeInternal == PortSetMode.SharedPort)
                {
                    return new IPort[]
                    {
                        SharedPortInternal
                    };
                }
                for (int i = 0; i < PortsTable.Length; i++)
                {
                    AllocatePort(i);
                }
                return PortsTable;
            }
        }

        public Port<object> SharedPort
        {
            get
            {
                return SharedPortInternal;
            }
        }

        public PortSetMode Mode
        {
            get
            {
                return ModeInternal;
            }
            set
            {
                ModeInternal = value;
                if (value == PortSetMode.SharedPort && SharedPortInternal == null)
                {
                    SharedPortInternal = new Port<object>();
                    return;
                }
                if (value == PortSetMode.Default && SharedPortInternal != null)
                {
                    SharedPortInternal = null;
                }
            }
        }

        public IPort this[Type portItemType]
        {
            get
            {
                for (int i = 0; i < PortsTable.Length; i++)
                {
                    if (Types[i] == portItemType)
                    {
                        return AllocatePort(i);
                    }
                }
                for (int j = 0; j < PortsTable.Length; j++)
                {
                    if (Types[j].IsAssignableFrom(portItemType))
                    {
                        return AllocatePort(j);
                    }
                }
                return null;
            }
        }

        protected PortSet()
        {
        }

        public PortSet(params Type[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            if (types.Length == 0)
            {
                throw new ArgumentOutOfRangeException("types");
            }
            int num = 0;
            PortsTable = new IPort[types.Length];
            Types = types;
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                Type type2 = typeof(Port<>).MakeGenericType(new Type[]
                {
                    type
                });
                PortsTable[num++] = (IPort)Activator.CreateInstance(type2);
            }
        }

        public void PostUnknownType(object item)
        {
            if (!TryPostUnknownType(item))
            {
                throw new PortNotFoundException(this, item);
            }
        }

        public bool TryPostUnknownType(object item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return true;
            }
            Type portItemType;
            if (!PortSet.FindTypeFromRuntimeType(item, Types, out portItemType))
            {
                return false;
            }
            IPort port = this[portItemType];
            return port != null && port.TryPostUnknownType(item);
        }

        public static bool FindTypeFromRuntimeType(object item, Type[] types, out Type portItemType)
        {
            portItemType = null;
            if (item == null)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (!types[i].IsValueType || (types[i].IsGenericType && !types[i].IsGenericTypeDefinition && types[i].GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        portItemType = types[i];
                        break;
                    }
                }
                return !(portItemType == null);
            }
            portItemType = item.GetType();
            return true;
        }

        public T Test<T>()
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                return (T)((object)SharedPortInternal.Test());
            }
            IPortReceive portReceive = this[typeof(T)] as IPortReceive;
            if (portReceive == null)
            {
                throw new PortNotFoundException();
            }
            return (T)((object)portReceive.Test());
        }

        private IPort AllocatePort(int portIndex)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                throw new InvalidOperationException();
            }
            if (PortsTable[portIndex] == null)
            {
                lock (this)
                {
                    if (PortsTable[portIndex] == null)
                    {
                        Type type = typeof(Port<>).MakeGenericType(new Type[]
                        {
                            Types[portIndex]
                        });
                        PortsTable[portIndex] = (IPort)Activator.CreateInstance(type);
                    }
                }
            }
            return PortsTable[portIndex];
        }

        protected Port<TYPE> AllocatePort<TYPE>()
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                throw new InvalidOperationException();
            }
            Type typeFromHandle = typeof(TYPE);
            Port<TYPE> result;
            try
            {
                IPort port = this[typeFromHandle];
                result = (Port<TYPE>)port;
            }
            catch
            {
                for (int i = 0; i < PortsTable.Length; i++)
                {
                    if (Types[i].IsAssignableFrom(typeFromHandle))
                    {
                        lock (this)
                        {
                            if (PortsTable[i] == null)
                            {
                                Port<TYPE> port2 = new Port<TYPE>();
                                PortsTable[i] = port2;
                                result = port2;
                                return result;
                            }
                            result = (Port<TYPE>)PortsTable[i];
                            return result;
                        }
                    }
                }
                throw new PortNotFoundException();
            }
            return result;
        }

        public static implicit operator Choice(PortSet portSet)
        {
            return PortSet.ImplicitChoiceOperator(portSet);
        }

        public static Choice ImplicitChoiceOperator(IPortSet portSet)
        {
            if (portSet == null)
            {
                throw new ArgumentNullException("portSet");
            }
            ICollection<IPort> ports = portSet.Ports;
            Receiver[] array = new Receiver[ports.Count];
            int num = 0;
            foreach (IPort current in portSet.Ports)
            {
                Receiver receiver = new Receiver(false, (IPortReceive)current, null);
                receiver.KeepItemInPort = true;
                array[num++] = receiver;
            }
            return new Choice(array);
        }
    }

    public class PortSet<T0, T1, T2> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[3];
            PortsTable = new IPort[3];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2)
        {
            PortsTable = new IPort[3];
            Types = new Type[3];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2> port)
        {
            return port.P2;
        }

        public static implicit operator T0(PortSet<T0, T1, T2> port)
        {
            return port.Test<T0>();
        }

        public static implicit operator T1(PortSet<T0, T1, T2> port)
        {
            return port.Test<T1>();
        }

        public static implicit operator T2(PortSet<T0, T1, T2> port)
        {
            return port.Test<T2>();
        }
    }

    public class PortSet<T0, T1, T2, T3> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[4];
            PortsTable = new IPort[4];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3)
        {
            PortsTable = new IPort[4];
            Types = new Type[4];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3> port)
        {
            return port.P3;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[5];
            PortsTable = new IPort[5];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4)
        {
            PortsTable = new IPort[5];
            Types = new Type[5];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4> port)
        {
            return port.P4;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[6];
            PortsTable = new IPort[6];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5)
        {
            PortsTable = new IPort[6];
            Types = new Type[6];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5> port)
        {
            return port.P5;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[7];
            PortsTable = new IPort[7];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6)
        {
            PortsTable = new IPort[7];
            Types = new Type[7];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6> port)
        {
            return port.P6;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[8];
            PortsTable = new IPort[8];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7)
        {
            PortsTable = new IPort[8];
            Types = new Type[8];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7> port)
        {
            return port.P7;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[9];
            PortsTable = new IPort[9];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8)
        {
            PortsTable = new IPort[9];
            Types = new Type[9];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8> port)
        {
            return port.P8;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[10];
            PortsTable = new IPort[10];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9)
        {
            PortsTable = new IPort[10];
            Types = new Type[10];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> port)
        {
            return port.P9;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[11];
            PortsTable = new IPort[11];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10)
        {
            PortsTable = new IPort[11];
            Types = new Type[11];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> port)
        {
            return port.P10;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[12];
            PortsTable = new IPort[12];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11)
        {
            PortsTable = new IPort[12];
            Types = new Type[12];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> port)
        {
            return port.P11;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[13];
            PortsTable = new IPort[13];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12)
        {
            PortsTable = new IPort[13];
            Types = new Type[13];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> port)
        {
            return port.P12;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[14];
            PortsTable = new IPort[14];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13)
        {
            PortsTable = new IPort[14];
            Types = new Type[14];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> port)
        {
            return port.P13;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public Port<T14> P14
        {
            get
            {
                return base.AllocatePort<T14>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[15];
            PortsTable = new IPort[15];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
            Types[num++] = typeof(T14);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13, Port<T14> parameter14)
        {
            PortsTable = new IPort[15];
            Types = new Type[15];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
            Types[num] = typeof(T14);
            PortsTable[num++] = parameter14;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public void Post(T14 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P14.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P13;
        }

        public static implicit operator Port<T14>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> port)
        {
            return port.P14;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public Port<T14> P14
        {
            get
            {
                return base.AllocatePort<T14>();
            }
        }

        public Port<T15> P15
        {
            get
            {
                return base.AllocatePort<T15>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[16];
            PortsTable = new IPort[16];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
            Types[num++] = typeof(T14);
            Types[num++] = typeof(T15);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13, Port<T14> parameter14, Port<T15> parameter15)
        {
            PortsTable = new IPort[16];
            Types = new Type[16];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
            Types[num] = typeof(T14);
            PortsTable[num++] = parameter14;
            Types[num] = typeof(T15);
            PortsTable[num++] = parameter15;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public void Post(T14 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P14.Post(item);
        }

        public void Post(T15 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P15.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P13;
        }

        public static implicit operator Port<T14>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P14;
        }

        public static implicit operator Port<T15>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> port)
        {
            return port.P15;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public Port<T14> P14
        {
            get
            {
                return base.AllocatePort<T14>();
            }
        }

        public Port<T15> P15
        {
            get
            {
                return base.AllocatePort<T15>();
            }
        }

        public Port<T16> P16
        {
            get
            {
                return base.AllocatePort<T16>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[17];
            PortsTable = new IPort[17];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
            Types[num++] = typeof(T14);
            Types[num++] = typeof(T15);
            Types[num++] = typeof(T16);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13, Port<T14> parameter14, Port<T15> parameter15, Port<T16> parameter16)
        {
            PortsTable = new IPort[17];
            Types = new Type[17];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
            Types[num] = typeof(T14);
            PortsTable[num++] = parameter14;
            Types[num] = typeof(T15);
            PortsTable[num++] = parameter15;
            Types[num] = typeof(T16);
            PortsTable[num++] = parameter16;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public void Post(T14 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P14.Post(item);
        }

        public void Post(T15 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P15.Post(item);
        }

        public void Post(T16 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P16.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P13;
        }

        public static implicit operator Port<T14>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P14;
        }

        public static implicit operator Port<T15>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P15;
        }

        public static implicit operator Port<T16>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> port)
        {
            return port.P16;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public Port<T14> P14
        {
            get
            {
                return base.AllocatePort<T14>();
            }
        }

        public Port<T15> P15
        {
            get
            {
                return base.AllocatePort<T15>();
            }
        }

        public Port<T16> P16
        {
            get
            {
                return base.AllocatePort<T16>();
            }
        }

        public Port<T17> P17
        {
            get
            {
                return base.AllocatePort<T17>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[18];
            PortsTable = new IPort[18];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
            Types[num++] = typeof(T14);
            Types[num++] = typeof(T15);
            Types[num++] = typeof(T16);
            Types[num++] = typeof(T17);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13, Port<T14> parameter14, Port<T15> parameter15, Port<T16> parameter16, Port<T17> parameter17)
        {
            PortsTable = new IPort[18];
            Types = new Type[18];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
            Types[num] = typeof(T14);
            PortsTable[num++] = parameter14;
            Types[num] = typeof(T15);
            PortsTable[num++] = parameter15;
            Types[num] = typeof(T16);
            PortsTable[num++] = parameter16;
            Types[num] = typeof(T17);
            PortsTable[num++] = parameter17;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public void Post(T14 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P14.Post(item);
        }

        public void Post(T15 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P15.Post(item);
        }

        public void Post(T16 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P16.Post(item);
        }

        public void Post(T17 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P17.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P13;
        }

        public static implicit operator Port<T14>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P14;
        }

        public static implicit operator Port<T15>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P15;
        }

        public static implicit operator Port<T16>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P16;
        }

        public static implicit operator Port<T17>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> port)
        {
            return port.P17;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public Port<T14> P14
        {
            get
            {
                return base.AllocatePort<T14>();
            }
        }

        public Port<T15> P15
        {
            get
            {
                return base.AllocatePort<T15>();
            }
        }

        public Port<T16> P16
        {
            get
            {
                return base.AllocatePort<T16>();
            }
        }

        public Port<T17> P17
        {
            get
            {
                return base.AllocatePort<T17>();
            }
        }

        public Port<T18> P18
        {
            get
            {
                return base.AllocatePort<T18>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[19];
            PortsTable = new IPort[19];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
            Types[num++] = typeof(T14);
            Types[num++] = typeof(T15);
            Types[num++] = typeof(T16);
            Types[num++] = typeof(T17);
            Types[num++] = typeof(T18);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13, Port<T14> parameter14, Port<T15> parameter15, Port<T16> parameter16, Port<T17> parameter17, Port<T18> parameter18)
        {
            PortsTable = new IPort[19];
            Types = new Type[19];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
            Types[num] = typeof(T14);
            PortsTable[num++] = parameter14;
            Types[num] = typeof(T15);
            PortsTable[num++] = parameter15;
            Types[num] = typeof(T16);
            PortsTable[num++] = parameter16;
            Types[num] = typeof(T17);
            PortsTable[num++] = parameter17;
            Types[num] = typeof(T18);
            PortsTable[num++] = parameter18;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public void Post(T14 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P14.Post(item);
        }

        public void Post(T15 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P15.Post(item);
        }

        public void Post(T16 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P16.Post(item);
        }

        public void Post(T17 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P17.Post(item);
        }

        public void Post(T18 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P18.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P13;
        }

        public static implicit operator Port<T14>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P14;
        }

        public static implicit operator Port<T15>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P15;
        }

        public static implicit operator Port<T16>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P16;
        }

        public static implicit operator Port<T17>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P17;
        }

        public static implicit operator Port<T18>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> port)
        {
            return port.P18;
        }
    }

    public class PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> : PortSet
    {
        public Port<T0> P0
        {
            get
            {
                return base.AllocatePort<T0>();
            }
        }

        public Port<T1> P1
        {
            get
            {
                return base.AllocatePort<T1>();
            }
        }

        public Port<T2> P2
        {
            get
            {
                return base.AllocatePort<T2>();
            }
        }

        public Port<T3> P3
        {
            get
            {
                return base.AllocatePort<T3>();
            }
        }

        public Port<T4> P4
        {
            get
            {
                return base.AllocatePort<T4>();
            }
        }

        public Port<T5> P5
        {
            get
            {
                return base.AllocatePort<T5>();
            }
        }

        public Port<T6> P6
        {
            get
            {
                return base.AllocatePort<T6>();
            }
        }

        public Port<T7> P7
        {
            get
            {
                return base.AllocatePort<T7>();
            }
        }

        public Port<T8> P8
        {
            get
            {
                return base.AllocatePort<T8>();
            }
        }

        public Port<T9> P9
        {
            get
            {
                return base.AllocatePort<T9>();
            }
        }

        public Port<T10> P10
        {
            get
            {
                return base.AllocatePort<T10>();
            }
        }

        public Port<T11> P11
        {
            get
            {
                return base.AllocatePort<T11>();
            }
        }

        public Port<T12> P12
        {
            get
            {
                return base.AllocatePort<T12>();
            }
        }

        public Port<T13> P13
        {
            get
            {
                return base.AllocatePort<T13>();
            }
        }

        public Port<T14> P14
        {
            get
            {
                return base.AllocatePort<T14>();
            }
        }

        public Port<T15> P15
        {
            get
            {
                return base.AllocatePort<T15>();
            }
        }

        public Port<T16> P16
        {
            get
            {
                return base.AllocatePort<T16>();
            }
        }

        public Port<T17> P17
        {
            get
            {
                return base.AllocatePort<T17>();
            }
        }

        public Port<T18> P18
        {
            get
            {
                return base.AllocatePort<T18>();
            }
        }

        public Port<T19> P19
        {
            get
            {
                return base.AllocatePort<T19>();
            }
        }

        public PortSet()
        {
            int num = 0;
            Types = new Type[20];
            PortsTable = new IPort[20];
            Types[num++] = typeof(T0);
            Types[num++] = typeof(T1);
            Types[num++] = typeof(T2);
            Types[num++] = typeof(T3);
            Types[num++] = typeof(T4);
            Types[num++] = typeof(T5);
            Types[num++] = typeof(T6);
            Types[num++] = typeof(T7);
            Types[num++] = typeof(T8);
            Types[num++] = typeof(T9);
            Types[num++] = typeof(T10);
            Types[num++] = typeof(T11);
            Types[num++] = typeof(T12);
            Types[num++] = typeof(T13);
            Types[num++] = typeof(T14);
            Types[num++] = typeof(T15);
            Types[num++] = typeof(T16);
            Types[num++] = typeof(T17);
            Types[num++] = typeof(T18);
            Types[num++] = typeof(T19);
        }

        public PortSet(PortSetMode mode) : this()
        {
            base.Mode = mode;
        }

        public PortSet(Port<T0> parameter0, Port<T1> parameter1, Port<T2> parameter2, Port<T3> parameter3, Port<T4> parameter4, Port<T5> parameter5, Port<T6> parameter6, Port<T7> parameter7, Port<T8> parameter8, Port<T9> parameter9, Port<T10> parameter10, Port<T11> parameter11, Port<T12> parameter12, Port<T13> parameter13, Port<T14> parameter14, Port<T15> parameter15, Port<T16> parameter16, Port<T17> parameter17, Port<T18> parameter18, Port<T19> parameter19)
        {
            PortsTable = new IPort[20];
            Types = new Type[20];
            int num = 0;
            Types[num] = typeof(T0);
            PortsTable[num++] = parameter0;
            Types[num] = typeof(T1);
            PortsTable[num++] = parameter1;
            Types[num] = typeof(T2);
            PortsTable[num++] = parameter2;
            Types[num] = typeof(T3);
            PortsTable[num++] = parameter3;
            Types[num] = typeof(T4);
            PortsTable[num++] = parameter4;
            Types[num] = typeof(T5);
            PortsTable[num++] = parameter5;
            Types[num] = typeof(T6);
            PortsTable[num++] = parameter6;
            Types[num] = typeof(T7);
            PortsTable[num++] = parameter7;
            Types[num] = typeof(T8);
            PortsTable[num++] = parameter8;
            Types[num] = typeof(T9);
            PortsTable[num++] = parameter9;
            Types[num] = typeof(T10);
            PortsTable[num++] = parameter10;
            Types[num] = typeof(T11);
            PortsTable[num++] = parameter11;
            Types[num] = typeof(T12);
            PortsTable[num++] = parameter12;
            Types[num] = typeof(T13);
            PortsTable[num++] = parameter13;
            Types[num] = typeof(T14);
            PortsTable[num++] = parameter14;
            Types[num] = typeof(T15);
            PortsTable[num++] = parameter15;
            Types[num] = typeof(T16);
            PortsTable[num++] = parameter16;
            Types[num] = typeof(T17);
            PortsTable[num++] = parameter17;
            Types[num] = typeof(T18);
            PortsTable[num++] = parameter18;
            Types[num] = typeof(T19);
            PortsTable[num++] = parameter19;
        }

        public void Post(T0 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P0.Post(item);
        }

        public void Post(T1 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P1.Post(item);
        }

        public void Post(T2 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P2.Post(item);
        }

        public void Post(T3 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P3.Post(item);
        }

        public void Post(T4 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P4.Post(item);
        }

        public void Post(T5 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P5.Post(item);
        }

        public void Post(T6 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P6.Post(item);
        }

        public void Post(T7 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P7.Post(item);
        }

        public void Post(T8 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P8.Post(item);
        }

        public void Post(T9 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P9.Post(item);
        }

        public void Post(T10 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P10.Post(item);
        }

        public void Post(T11 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P11.Post(item);
        }

        public void Post(T12 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P12.Post(item);
        }

        public void Post(T13 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P13.Post(item);
        }

        public void Post(T14 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P14.Post(item);
        }

        public void Post(T15 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P15.Post(item);
        }

        public void Post(T16 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P16.Post(item);
        }

        public void Post(T17 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P17.Post(item);
        }

        public void Post(T18 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P18.Post(item);
        }

        public void Post(T19 item)
        {
            if (ModeInternal == PortSetMode.SharedPort)
            {
                SharedPortInternal.Post(item);
                return;
            }
            P19.Post(item);
        }

        public static implicit operator Port<T0>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P0;
        }

        public static implicit operator Port<T1>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P1;
        }

        public static implicit operator Port<T2>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P2;
        }

        public static implicit operator Port<T3>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P3;
        }

        public static implicit operator Port<T4>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P4;
        }

        public static implicit operator Port<T5>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P5;
        }

        public static implicit operator Port<T6>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P6;
        }

        public static implicit operator Port<T7>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P7;
        }

        public static implicit operator Port<T8>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P8;
        }

        public static implicit operator Port<T9>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P9;
        }

        public static implicit operator Port<T10>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P10;
        }

        public static implicit operator Port<T11>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P11;
        }

        public static implicit operator Port<T12>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P12;
        }

        public static implicit operator Port<T13>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P13;
        }

        public static implicit operator Port<T14>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P14;
        }

        public static implicit operator Port<T15>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P15;
        }

        public static implicit operator Port<T16>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P16;
        }

        public static implicit operator Port<T17>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P17;
        }

        public static implicit operator Port<T18>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P18;
        }

        public static implicit operator Port<T19>(PortSet<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> port)
        {
            return port.P19;
        }
    }
}