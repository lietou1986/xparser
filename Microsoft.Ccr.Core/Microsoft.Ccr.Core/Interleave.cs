using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public class Interleave : IArbiterTask, ITask
    {
        private ArbiterTaskState _state;

        private List<ReceiverTask> _mutexBranches;

        private List<ReceiverTask> _concurrentBranches;

        private object _final;

        private int _mutexActive;

        private int _concurrentActive;

        private Handler _ArbiterCleanupHandler;

        private DispatcherQueue _dispatcherQueue;

        private int _nextMutexQueueIndex;

        private int _nextConcurrentQueueIndex;

        public int PendingExclusiveCount
        {
            get
            {
                int result;
                lock (_mutexBranches)
                {
                    result = CountAllPendingItems(_mutexBranches);
                }
                return result;
            }
        }

        public int PendingConcurrentCount
        {
            get
            {
                int result;
                lock (_mutexBranches)
                {
                    result = CountAllPendingItems(_concurrentBranches);
                }
                return result;
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
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public ArbiterTaskState ArbiterState
        {
            get
            {
                return _state;
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

        private int CountAllPendingItems(List<ReceiverTask> receivers)
        {
            int num = 0;
            foreach (ReceiverTask current in receivers)
            {
                InterleaveReceiverContext interleaveReceiverContext = current.ArbiterContext as InterleaveReceiverContext;
                num += interleaveReceiverContext.PendingItems.Count;
            }
            return num;
        }

        public override string ToString()
        {
            string text = null;
            if (_mutexActive == 0 && _concurrentActive == 0)
            {
                text = "Idle";
            }
            else if (_mutexActive == -1 && _concurrentActive > 0)
            {
                text = "Concurrent Active with Exclusive pending";
            }
            else if (_mutexActive == 1 && _concurrentActive > 0)
            {
                text = "Exclusive active with Concurrent Active";
            }
            else if (_mutexActive == 1 && _concurrentActive == 0)
            {
                text = "Exclusive active";
            }
            return string.Format(CultureInfo.InvariantCulture, "\t{0}({1}) guarding {2} Exclusive and {3} Concurrent branches", new object[]
            {
                base.GetType().Name,
                text,
                _mutexBranches.Count,
                _concurrentBranches.Count
            });
        }

        public Interleave()
        {
        }

        public Interleave(TeardownReceiverGroup teardown, ExclusiveReceiverGroup mutex, ConcurrentReceiverGroup concurrent)
        {
            ReceiverTask[] branches = teardown._branches;
            for (int i = 0; i < branches.Length; i++)
            {
                ReceiverTask receiverTask = branches[i];
                receiverTask.ArbiterContext = new InterleaveReceiverContext(InterleaveReceivers.Teardown);
            }
            ReceiverTask[] branches2 = mutex._branches;
            for (int j = 0; j < branches2.Length; j++)
            {
                ReceiverTask receiverTask2 = branches2[j];
                receiverTask2.ArbiterContext = new InterleaveReceiverContext(InterleaveReceivers.Exclusive);
            }
            ReceiverTask[] branches3 = concurrent._branches;
            for (int k = 0; k < branches3.Length; k++)
            {
                ReceiverTask receiverTask3 = branches3[k];
                receiverTask3.ArbiterContext = new InterleaveReceiverContext(InterleaveReceivers.Concurrent);
            }
            _mutexBranches = new List<ReceiverTask>(teardown._branches);
            ReceiverTask[] branches4 = mutex._branches;
            for (int l = 0; l < branches4.Length; l++)
            {
                ReceiverTask item = branches4[l];
                _mutexBranches.Add(item);
            }
            _concurrentBranches = new List<ReceiverTask>(concurrent._branches);
        }

        public Interleave(ExclusiveReceiverGroup mutex, ConcurrentReceiverGroup concurrent) : this(new TeardownReceiverGroup(new ReceiverTask[0]), mutex, concurrent)
        {
        }

        public ITask PartialClone()
        {
            throw new NotSupportedException();
        }

        public IEnumerator<ITask> Execute()
        {
            _state = ArbiterTaskState.Active;
            Register();
            return null;
        }

        private void Register()
        {
            lock (_mutexBranches)
            {
                foreach (ReceiverTask current in _mutexBranches)
                {
                    current.Arbiter = this;
                }
                foreach (ReceiverTask current2 in _concurrentBranches)
                {
                    current2.Arbiter = this;
                }
            }
        }

        public void CombineWith(Interleave child)
        {
            if (_state == ArbiterTaskState.Done)
            {
                throw new InvalidOperationException("Parent Interleave context is no longer active");
            }
            List<ReceiverTask> list = null;
            List<ReceiverTask> list2 = null;
            lock (child._mutexBranches)
            {
                list = new List<ReceiverTask>(child._mutexBranches);
                list2 = new List<ReceiverTask>(child._concurrentBranches);
                foreach (ReceiverTask current in list2)
                {
                    if (current.State == ReceiverTaskState.Onetime)
                    {
                        throw new InvalidOperationException("Concurrent Receivers must be Reissue");
                    }
                }
                child._mutexBranches = null;
                child._concurrentBranches = null;
            }
            lock (_mutexBranches)
            {
                _mutexBranches.AddRange(list);
                _concurrentBranches.AddRange(list2);
            }
            list.ForEach(delegate (ReceiverTask receiver)
            {
                receiver.Arbiter = this;
            });
            list2.ForEach(delegate (ReceiverTask receiver)
            {
                receiver.Arbiter = this;
            });
        }

        private void CleanupPending(List<ReceiverTask> receivers)
        {
            foreach (ReceiverTask current in receivers)
            {
                InterleaveReceiverContext interleaveReceiverContext = current.ArbiterContext as InterleaveReceiverContext;
                foreach (Tuple<ITask, ReceiverTask> current2 in interleaveReceiverContext.PendingItems)
                {
                    current2.Item1.Cleanup(current2.Item0);
                }
                interleaveReceiverContext.PendingItems.Clear();
            }
        }

        public void Cleanup(ITask winner)
        {
            foreach (ReceiverTask current in _concurrentBranches)
            {
                current.Cleanup();
            }
            foreach (ReceiverTask current2 in _mutexBranches)
            {
                current2.Cleanup();
            }
            lock (_mutexBranches)
            {
                CleanupPending(_concurrentBranches);
                CleanupPending(_mutexBranches);
            }
            _dispatcherQueue.Enqueue(winner);
        }

        public bool Evaluate(ReceiverTask receiver, ref ITask deferredTask)
        {
            if (_state == ArbiterTaskState.Done)
            {
                deferredTask = null;
                return false;
            }
            lock (_mutexBranches)
            {
                if (((InterleaveReceiverContext)receiver.ArbiterContext).ReceiverGroup == InterleaveReceivers.Teardown && receiver.State == ReceiverTaskState.Onetime)
                {
                    _state = ArbiterTaskState.Done;
                    object obj = Interlocked.CompareExchange(ref _final, deferredTask, null);
                    if (obj != null)
                    {
                        deferredTask = null;
                        return false;
                    }
                }
                bool flag2 = ((InterleaveReceiverContext)receiver.ArbiterContext).ReceiverGroup != InterleaveReceivers.Concurrent;
                bool flag3 = Arbitrate(flag2);
                if (flag2)
                {
                    if (flag3)
                    {
                        if (_final == deferredTask)
                        {
                            _final = null;
                            deferredTask = new Task<ITask>(deferredTask, new Handler<ITask>(Cleanup));
                        }
                        else
                        {
                            deferredTask.ArbiterCleanupHandler = new Handler(ExclusiveFinalizer);
                        }
                    }
                    else
                    {
                        if (deferredTask != _final)
                        {
                            ((InterleaveReceiverContext)receiver.ArbiterContext).PendingItems.Enqueue(new Tuple<ITask, ReceiverTask>(deferredTask, receiver));
                        }
                        deferredTask = null;
                    }
                    if (deferredTask != null)
                    {
                        receiver.TaskQueue.Enqueue(deferredTask);
                        deferredTask = null;
                    }
                }
                else if (flag3)
                {
                    deferredTask.ArbiterCleanupHandler = new Handler(ConcurrentFinalizer);
                }
                else
                {
                    ((InterleaveReceiverContext)receiver.ArbiterContext).PendingItems.Enqueue(new Tuple<ITask, ReceiverTask>(deferredTask, receiver));
                    deferredTask = null;
                }
            }
            return true;
        }

        private bool Arbitrate(bool IsExclusive)
        {
            if (IsExclusive)
            {
                if (_mutexActive == 0)
                {
                    if (_concurrentActive > 0)
                    {
                        _mutexActive = -1;
                        return false;
                    }
                    _mutexActive = 1;
                    return true;
                }
                else if (_mutexActive == -1 && _concurrentActive == 0)
                {
                    _mutexActive = 1;
                    return true;
                }
            }
            else if (_mutexActive == 0)
            {
                _concurrentActive++;
                return true;
            }
            return false;
        }

        private void ExclusiveFinalizer()
        {
            ProcessAllPending(true);
        }

        private void ConcurrentFinalizer()
        {
            ProcessAllPending(false);
        }

        private void ProcessAllPending(bool exclusiveJustFinished)
        {
            ITask task = null;
            lock (_mutexBranches)
            {
                if (_state == ArbiterTaskState.Done)
                {
                    if (_final == null)
                    {
                        return;
                    }
                    task = (ITask)_final;
                }
            }
            ITask task2 = null;
            lock (_mutexBranches)
            {
                if (exclusiveJustFinished)
                {
                    _mutexActive = 0;
                }
                else
                {
                    _concurrentActive--;
                }
                if (task == null)
                {
                    task2 = ProcessPending(true, _mutexBranches);
                }
            }
            if (task2 == null)
            {
                while (true)
                {
                    lock (_mutexBranches)
                    {
                        task2 = ProcessPending(false, _concurrentBranches);
                    }
                    if (task2 == null)
                    {
                        break;
                    }
                    task2.ArbiterCleanupHandler = new Handler(ConcurrentFinalizer);
                    _dispatcherQueue.Enqueue(task2);
                }
                if (task != null)
                {
                    lock (_mutexBranches)
                    {
                        if (_concurrentActive == 0 && _mutexActive <= 0)
                        {
                            _final = null;
                        }
                    }
                    if (_final == null && task != null)
                    {
                        _dispatcherQueue.Enqueue(new Task<ITask>(task, new Handler<ITask>(Cleanup)));
                    }
                }
                return;
            }
            task2.ArbiterCleanupHandler = new Handler(ExclusiveFinalizer);
            _dispatcherQueue.Enqueue(task2);
        }

        private ITask ProcessPending(bool IsExclusive, List<ReceiverTask> receivers)
        {
            int num = IsExclusive ? _mutexBranches.Count : _concurrentBranches.Count;
            if (num == 0)
            {
                return null;
            }
            int num2 = num;
            while (--num2 >= 0)
            {
                int index;
                if (IsExclusive)
                {
                    _nextMutexQueueIndex = (_nextMutexQueueIndex + 1) % num;
                    index = _nextMutexQueueIndex;
                }
                else
                {
                    _nextConcurrentQueueIndex = (_nextConcurrentQueueIndex + 1) % num;
                    index = _nextConcurrentQueueIndex;
                }
                Queue<Tuple<ITask, ReceiverTask>> pendingItems = ((InterleaveReceiverContext)receivers[index].ArbiterContext).PendingItems;
                if (pendingItems.Count > 0 && Arbitrate(IsExclusive))
                {
                    Tuple<ITask, ReceiverTask> tuple = pendingItems.Dequeue();
                    return tuple.Item0;
                }
            }
            return null;
        }

        public ITask TryDequeuePendingTask(InterleaveReceivers receiverMask)
        {
            lock (_mutexBranches)
            {
                if ((receiverMask & InterleaveReceivers.Exclusive) > (InterleaveReceivers)0)
                {
                    ITask result = Interleave.DequeuePendingItem(_mutexBranches);
                    return result;
                }
                if ((receiverMask & InterleaveReceivers.Concurrent) > (InterleaveReceivers)0)
                {
                    ITask result = Interleave.DequeuePendingItem(_concurrentBranches);
                    return result;
                }
            }
            return null;
        }

        public ITask TryDequeuePendingTask(ReceiverTask receiver, int queueDepthMin)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            if (!(receiver.ArbiterContext is InterleaveReceiverContext))
            {
                throw new ArgumentException("receiver", Resource1.InterleaveInvalidReceiverTaskArgumentForTryDequeuePendingItems);
            }
            if (queueDepthMin <= 0)
            {
                throw new ArgumentOutOfRangeException("queueDepthMin");
            }
            ITask result;
            lock (_mutexBranches)
            {
                result = Interleave.DequeuePendingItem(receiver, queueDepthMin);
            }
            return result;
        }

        private static ITask DequeuePendingItem(List<ReceiverTask> receivers)
        {
            foreach (ReceiverTask current in receivers)
            {
                ITask task = Interleave.DequeuePendingItem(current, 1);
                if (task != null)
                {
                    return task;
                }
            }
            return null;
        }

        private static ITask DequeuePendingItem(ReceiverTask receiver, int queueDepthMin)
        {
            Queue<Tuple<ITask, ReceiverTask>> pendingItems = ((InterleaveReceiverContext)receiver.ArbiterContext).PendingItems;
            if (pendingItems.Count >= queueDepthMin)
            {
                return pendingItems.Dequeue().Item0;
            }
            return null;
        }
    }
}