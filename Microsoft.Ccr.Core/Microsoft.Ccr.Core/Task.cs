using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Ccr.Core
{
    public class Task : TaskCommon
    {
        private CausalityThreadContext _causalityContext;

        private Handler _handler;

        public Handler Handler
        {
            get
            {
                return _handler;
            }
        }

        public override IPortElement this[int index]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int PortElementCount
        {
            get
            {
                return 0;
            }
        }

        public Task(Handler handler)
        {
            _handler = handler;
            _causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        public override ITask PartialClone()
        {
            return new Task(_handler);
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            Dispatcher.SetCurrentThreadCausalities(_causalityContext);
            _handler();
            return null;
        }
    }

    public class Task<T0> : TaskCommon
    {
        private readonly Handler<T0> _Handler;

        protected PortElement<T0> Param0;

        public override IPortElement this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return Param0;
                }
                throw new ArgumentException("parameter out of range", "index");
            }
            set
            {
                if (index == 0)
                {
                    Param0 = (PortElement<T0>)value;
                    return;
                }
                throw new ArgumentException("parameter out of range", "index");
            }
        }

        public override int PortElementCount
        {
            get
            {
                return 1;
            }
        }

        public Task(Handler<T0> handler)
        {
            _Handler = handler;
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        public override ITask PartialClone()
        {
            return new Task<T0>(_Handler);
        }

        public Task(T0 t0, Handler<T0> handler)
        {
            _Handler = handler;
            Param0 = new PortElement<T0>(t0);
            Param0._causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            _Handler(Param0.TypedItem);
            return null;
        }
    }

    public class Task<T0, T1> : TaskCommon
    {
        private readonly Handler<T0, T1> _Handler;

        protected PortElement<T0> Param0;

        protected PortElement<T1> Param1;

        public override IPortElement this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Param0;

                    case 1:
                        return Param1;

                    default:
                        throw new ArgumentException("parameter out of range", "index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Param0 = (PortElement<T0>)value;
                        return;

                    case 1:
                        Param1 = (PortElement<T1>)value;
                        return;

                    default:
                        throw new ArgumentException("parameter out of range", "index");
                }
            }
        }

        public override int PortElementCount
        {
            get
            {
                return 2;
            }
        }

        public Task(Handler<T0, T1> handler)
        {
            _Handler = handler;
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        public override ITask PartialClone()
        {
            return new Task<T0, T1>(_Handler);
        }

        public Task(T0 t0, T1 t1, Handler<T0, T1> handler)
        {
            _Handler = handler;
            Param0 = new PortElement<T0>(t0);
            Param1 = new PortElement<T1>(t1);
            Param0._causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            _Handler(Param0.TypedItem, Param1.TypedItem);
            return null;
        }
    }

    public class Task<T0, T1, T2> : TaskCommon
    {
        private readonly Handler<T0, T1, T2> _Handler;

        protected PortElement<T0> Param0;

        protected PortElement<T1> Param1;

        protected PortElement<T2> Param2;

        public override IPortElement this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Param0;

                    case 1:
                        return Param1;

                    case 2:
                        return Param2;

                    default:
                        throw new ArgumentException("parameter out of range", "index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Param0 = (PortElement<T0>)value;
                        return;

                    case 1:
                        Param1 = (PortElement<T1>)value;
                        return;

                    case 2:
                        Param2 = (PortElement<T2>)value;
                        return;

                    default:
                        throw new ArgumentException("parameter out of range", "index");
                }
            }
        }

        public override int PortElementCount
        {
            get
            {
                return 3;
            }
        }

        public Task(Handler<T0, T1, T2> handler)
        {
            _Handler = handler;
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        public override ITask PartialClone()
        {
            return new Task<T0, T1, T2>(_Handler);
        }

        public Task(T0 t0, T1 t1, T2 t2, Handler<T0, T1, T2> handler)
        {
            _Handler = handler;
            Param0 = new PortElement<T0>(t0);
            Param1 = new PortElement<T1>(t1);
            Param2 = new PortElement<T2>(t2);
            Param0._causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            _Handler(Param0.TypedItem, Param1.TypedItem, Param2.TypedItem);
            return null;
        }
    }
}