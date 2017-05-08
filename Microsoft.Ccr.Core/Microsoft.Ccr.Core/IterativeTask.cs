using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Ccr.Core
{
    public class IterativeTask : TaskCommon
    {
        private CausalityThreadContext _causalityContext;

        private IteratorHandler _Handler;

        public IteratorHandler Handler
        {
            get
            {
                return _Handler;
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

        public IterativeTask(IteratorHandler handler)
        {
            _Handler = handler;
            _causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        public override ITask PartialClone()
        {
            return new IterativeTask(_Handler);
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            Dispatcher.SetCurrentThreadCausalities(_causalityContext);
            return _Handler();
        }
    }

    public class IterativeTask<T0> : TaskCommon
    {
        private readonly IteratorHandler<T0> _Handler;

        private PortElement<T0> _Param0;

        public override IPortElement this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return _Param0;
                }
                throw new ArgumentException("parameter out of range", "index");
            }
            set
            {
                if (index == 0)
                {
                    _Param0 = (PortElement<T0>)value;
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

        public IterativeTask(IteratorHandler<T0> handler)
        {
            _Handler = handler;
        }

        public override ITask PartialClone()
        {
            return new IterativeTask<T0>(_Handler);
        }

        public IterativeTask(T0 t0, IteratorHandler<T0> handler)
        {
            _Handler = handler;
            _Param0 = new PortElement<T0>(t0);
            _Param0._causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            return _Handler(_Param0.TypedItem);
        }
    }

    public class IterativeTask<T0, T1> : TaskCommon
    {
        private readonly IteratorHandler<T0, T1> _Handler;

        private PortElement<T0> _Param0;

        private PortElement<T1> _Param1;

        public override IPortElement this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _Param0;

                    case 1:
                        return _Param1;

                    default:
                        throw new ArgumentException("parameter out of range", "index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _Param0 = (PortElement<T0>)value;
                        return;

                    case 1:
                        _Param1 = (PortElement<T1>)value;
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

        public IterativeTask(IteratorHandler<T0, T1> handler)
        {
            _Handler = handler;
        }

        public override ITask PartialClone()
        {
            return new IterativeTask<T0, T1>(_Handler);
        }

        public IterativeTask(T0 t0, T1 t1, IteratorHandler<T0, T1> handler)
        {
            _Handler = handler;
            _Param0 = new PortElement<T0>(t0);
            _Param1 = new PortElement<T1>(t1);
            _Param0._causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            return _Handler(_Param0.TypedItem, _Param1.TypedItem);
        }
    }

    public class IterativeTask<T0, T1, T2> : TaskCommon
    {
        private readonly IteratorHandler<T0, T1, T2> _Handler;

        private PortElement<T0> _Param0;

        private PortElement<T1> _Param1;

        private PortElement<T2> _Param2;

        public override IPortElement this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _Param0;

                    case 1:
                        return _Param1;

                    case 2:
                        return _Param2;

                    default:
                        throw new ArgumentException("parameter out of range", "index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _Param0 = (PortElement<T0>)value;
                        return;

                    case 1:
                        _Param1 = (PortElement<T1>)value;
                        return;

                    case 2:
                        _Param2 = (PortElement<T2>)value;
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

        public IterativeTask(IteratorHandler<T0, T1, T2> handler)
        {
            _Handler = handler;
        }

        public override ITask PartialClone()
        {
            return new IterativeTask<T0, T1, T2>(_Handler);
        }

        public IterativeTask(T0 t0, T1 t1, T2 t2, IteratorHandler<T0, T1, T2> handler)
        {
            _Handler = handler;
            _Param0 = new PortElement<T0>(t0);
            _Param1 = new PortElement<T1>(t1);
            _Param2 = new PortElement<T2>(t2);
            _Param0._causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread();
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        [DebuggerNonUserCode, DebuggerStepThrough]
        public override IEnumerator<ITask> Execute()
        {
            return _Handler(_Param0.TypedItem, _Param1.TypedItem, _Param2.TypedItem);
        }
    }
}