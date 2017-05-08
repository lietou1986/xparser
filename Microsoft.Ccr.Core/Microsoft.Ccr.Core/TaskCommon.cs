using Microsoft.Ccr.Core.Arbiters;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public abstract class TaskCommon : ITask
    {
        internal TaskCommon _previous;

        internal TaskCommon _next;

        private Handler _ArbiterCleanupHandler;

        private object _linkedIterator;

        private DispatcherQueue _dispatcherQueue;

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

        public abstract IPortElement this[int index]
        {
            get;
            set;
        }

        public abstract int PortElementCount
        {
            get;
        }

        public abstract ITask PartialClone();

        public abstract IEnumerator<ITask> Execute();
    }
}