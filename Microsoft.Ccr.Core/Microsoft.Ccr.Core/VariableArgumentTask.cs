using Microsoft.Ccr.Core.Arbiters;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public sealed class VariableArgumentTask<T> : ITask
    {
        private Handler _ArbiterCleanupHandler;

        private object _linkedIterator;

        private DispatcherQueue _dispatcherQueue;

        private readonly VariableArgumentHandler<T> _Handler;

        private readonly IPortElement[] _aParams;

        public Handler ArbiterCleanupHandler
        {
            get
            {
                return _ArbiterCleanupHandler;
            }
            set
            {
                _ArbiterCleanupHandler = value;
            }
        }

        public object LinkedIterator
        {
            get
            {
                return _linkedIterator;
            }
            set
            {
                _linkedIterator = value;
            }
        }

        public DispatcherQueue TaskQueue
        {
            get
            {
                return _dispatcherQueue;
            }
            set
            {
                _dispatcherQueue = value;
            }
        }

        public IPortElement this[int index]
        {
            get
            {
                return _aParams[index];
            }
            set
            {
                _aParams[index] = value;
            }
        }

        public int PortElementCount
        {
            get
            {
                return _aParams.Length;
            }
        }

        public VariableArgumentTask(int varArgSize, VariableArgumentHandler<T> handler)
        {
            _Handler = handler;
            _aParams = new IPortElement[varArgSize];
        }

        public ITask PartialClone()
        {
            return new VariableArgumentTask<T>(_aParams.Length, _Handler);
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        public IEnumerator<ITask> Execute()
        {
            int num = _aParams.Length;
            T[] array = new T[num];
            while (--num >= 0)
            {
                array[num] = (T)((object)_aParams[num].Item);
            }
            _Handler(array);
            return null;
        }
    }

    public sealed class VariableArgumentTask<T0, T> : ITask
    {
        private Handler _ArbiterCleanupHandler;

        private object _linkedIterator;

        private DispatcherQueue _dispatcherQueue;

        private readonly VariableArgumentHandler<T0, T> _Handler;

        private IPortElement _Param0;

        private readonly IPortElement[] _aParams;

        public Handler ArbiterCleanupHandler
        {
            get
            {
                return _ArbiterCleanupHandler;
            }
            set
            {
                _ArbiterCleanupHandler = value;
            }
        }

        public object LinkedIterator
        {
            get
            {
                return _linkedIterator;
            }
            set
            {
                _linkedIterator = value;
            }
        }

        public DispatcherQueue TaskQueue
        {
            get
            {
                return _dispatcherQueue;
            }
            set
            {
                _dispatcherQueue = value;
            }
        }

        public IPortElement this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return _Param0;
                }
                return _aParams[index - 1];
            }
            set
            {
                if (index == 0)
                {
                    _Param0 = value;
                    return;
                }
                _aParams[index - 1] = value;
            }
        }

        public int PortElementCount
        {
            get
            {
                return 1 + _aParams.Length;
            }
        }

        public VariableArgumentTask(int varArgSize, VariableArgumentHandler<T0, T> handler)
        {
            _Handler = handler;
            _aParams = new IPortElement[varArgSize];
        }

        public ITask PartialClone()
        {
            return new VariableArgumentTask<T0, T>(_aParams.Length, _Handler);
        }

        public override string ToString()
        {
            if (_Handler.Target == null)
            {
                return "unknown:" + _Handler.Method.Name;
            }
            return _Handler.Target.ToString() + ":" + _Handler.Method.Name;
        }

        public IEnumerator<ITask> Execute()
        {
            int num = _aParams.Length;
            T[] array = new T[num];
            while (--num >= 0)
            {
                array[num] = (T)((object)_aParams[num].Item);
            }
            _Handler((T0)((object)_Param0.Item), array);
            return null;
        }
    }
}