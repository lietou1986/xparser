using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public class Choice : IArbiterTask, ITask
    {
        private enum ChoiceStage
        {
            Initialized,
            Pending,
            Commited,
            PostCommit0,
            PostCommit1,
            PostCommit2,
            PostCommit3,
            PostCommit4
        }

        private List<ReceiverTask> _branches;

        private int _stage;

        private DispatcherQueue _dispatcherQueue;

        private Handler _ArbiterCleanupHandler;

        private object _linkedIterator;

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

        public ArbiterTaskState ArbiterState
        {
            get
            {
                if (_stage >= 2)
                {
                    return ArbiterTaskState.Done;
                }
                if (_stage == 0)
                {
                    return ArbiterTaskState.Created;
                }
                return ArbiterTaskState.Active;
            }
        }

        public IPortElement this[int index]
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

        public int PortElementCount
        {
            get
            {
                return 0;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "\tChoice ({1}) with {0} branches", new object[]
            {
                _branches.Count,
                Enum.GetName(typeof(Choice.ChoiceStage), _stage)
            });
        }

        public Choice(params ReceiverTask[] branches)
        {
            if (branches == null)
            {
                throw new ArgumentNullException("branches");
            }
            if (_stage != 0)
            {
                throw new InvalidOperationException(Resource1.ChoiceAlreadyActiveException);
            }
            for (int i = 0; i < branches.Length; i++)
            {
                ReceiverTask receiverTask = branches[i];
                if (receiverTask.State == ReceiverTaskState.Persistent)
                {
                    throw new ArgumentOutOfRangeException("branches", Resource1.ChoiceBranchesCannotBePersisted);
                }
            }
            _branches = new List<ReceiverTask>(branches);
        }

        public ITask PartialClone()
        {
            throw new NotSupportedException();
        }

        public IEnumerator<ITask> Execute()
        {
            _stage++;
            foreach (ReceiverTask current in _branches)
            {
                current.Arbiter = this;
            }
            return null;
        }

        private void Cleanup(ITask winner)
        {
            foreach (ReceiverTask current in _branches)
            {
                current.Cleanup();
            }
            winner.LinkedIterator = LinkedIterator;
            winner.ArbiterCleanupHandler = ArbiterCleanupHandler;
            TaskQueue.Enqueue(winner);
        }

        public bool Evaluate(ReceiverTask receiver, ref ITask deferredTask)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            Choice.ChoiceStage choiceStage = (Choice.ChoiceStage)Interlocked.Increment(ref _stage);
            if (choiceStage == Choice.ChoiceStage.Commited)
            {
                Task<ITask> task = new Task<ITask>(deferredTask, new Handler<ITask>(Cleanup));
                deferredTask = task;
                return true;
            }
            deferredTask = null;
            return false;
        }
    }
}