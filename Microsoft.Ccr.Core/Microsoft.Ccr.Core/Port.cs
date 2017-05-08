using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Microsoft.Ccr.Core
{
    public class Port<T> : IPort, IPortReceive, IPortArbiterAccess
    {
        private Store<T> Store = new Store<T>();

        private PortMode _mode;

        public PortMode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                if (Store.ActiveReceiver == null)
                {
                    return;
                }
                if (Store.ActiveReceiver.State != ReceiverTaskState.Persistent)
                {
                    throw new InvalidOperationException();
                }
                if (Store.ReceiverCount > 1)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public virtual int ItemCount
        {
            get
            {
                return Store.ElementCount;
            }
        }

        public Port()
        {
            Store.Identity = Interlocked.Increment(ref StoreBase.Counter);
        }

        protected virtual ReceiverTask[] GetReceivers()
        {
            ReceiverTask[] result;
            lock (Store)
            {
                if (Store.ReceiverCount == 0)
                {
                    result = new ReceiverTask[0];
                }
                else
                {
                    result = Store.ReceiverListAsObjectArray;
                }
            }
            return result;
        }

        ReceiverTask[] IPortReceive.GetReceivers()
        {
            return GetReceivers();
        }

        protected virtual void RegisterReceiver(ReceiverTask receiver)
        {
            lock (Store)
            {
                if (!Store.IsElementListEmpty)
                {
                    PortElement<T> portElement = Store.ElementListFirst;
                    int num = Store.ElementCount;
                    while (true)
                    {
                        PortElement<T> next = portElement._next;
                        ITask task = null;
                        bool flag2 = receiver.Evaluate(portElement, ref task);
                        if (task != null)
                        {
                            receiver.TaskQueue.Enqueue(task);
                        }
                        if (flag2)
                        {
                            Store.ElementListRemove(portElement);
                        }
                        if (receiver.State != ReceiverTaskState.Persistent)
                        {
                            if (flag2)
                            {
                                break;
                            }
                            if (task != null)
                            {
                                goto IL_88;
                            }
                        }
                        portElement = next;
                        num--;
                        if (num <= 0)
                        {
                            goto IL_88;
                        }
                    }
                    return;
                }
                IL_88:
                if (_mode == PortMode.OptimizedSingleReissueReceiver && Store.ReceiverCount == 1)
                {
                    throw new InvalidOperationException("PortMode.OptimizedSingleReissueReceiver allows only a single receiver");
                }
                Store.AddReceiver(receiver);
            }
        }

        void IPortReceive.RegisterReceiver(ReceiverTask receiver)
        {
            RegisterReceiver(receiver);
        }

        protected virtual void UnregisterReceiver(ReceiverTask receiver)
        {
            lock (Store)
            {
                Store.RemoveReceiver(receiver);
            }
        }

        void IPortReceive.UnregisterReceiver(ReceiverTask receiver)
        {
            UnregisterReceiver(receiver);
        }

        public virtual void PostUnknownType(object item)
        {
            Post((T)((object)item));
        }

        internal static bool HasConversionFromNull()
        {
            return !typeof(T).IsValueType || Port<T>.IsNullableType();
        }

        private static bool IsNullableType()
        {
            return typeof(T).IsGenericType && !typeof(T).IsGenericTypeDefinition && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public virtual bool TryPostUnknownType(object item)
        {
            if ((item == null && Port<T>.HasConversionFromNull()) || typeof(T).IsAssignableFrom(item.GetType()))
            {
                Post((T)((object)item));
                return true;
            }
            return false;
        }

        public void PostElement(IPortElement element)
        {
            PostInternal(true, (PortElement<T>)element);
        }

        public virtual void Post(T item)
        {
            if (_mode == PortMode.OptimizedSingleReissueReceiver)
            {
                PostInternalFast(item);
                return;
            }
            PostInternal(false, new PortElement<T>(item, this)
            {
                _causalityContext = Dispatcher.CloneCausalitiesFromCurrentThread()
            });
        }

        object[] IPortReceive.GetItems()
        {
            return GetItems();
        }

        protected virtual object[] GetItems()
        {
            object[] elementListAsObjectArray;
            lock (Store)
            {
                elementListAsObjectArray = Store.ElementListAsObjectArray;
            }
            return elementListAsObjectArray;
        }

        public virtual object Test()
        {
            T t;
            if (Test(out t))
            {
                return t;
            }
            return null;
        }

        public IPortElement TestForElement()
        {
            PortElement<T> result;
            if (TestInternal(out result))
            {
                return result;
            }
            return null;
        }

        public IPortElement[] TestForMultipleElements(int count)
        {
            IPortElement[] result;
            lock (Store)
            {
                if (Store.ElementCount < count)
                {
                    result = null;
                }
                else
                {
                    IPortElement[] array = new IPortElement[count];
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = Store.ElementListRemoveFirst();
                    }
                    result = array;
                }
            }
            return result;
        }

        public virtual bool Test(out T item)
        {
            PortElement<T> portElement;
            if (TestInternal(out portElement))
            {
                item = portElement._item;
                return true;
            }
            item = default(T);
            return false;
        }

        public static implicit operator T(Port<T> port)
        {
            T result;
            port.Test(out result);
            return result;
        }

        public override string ToString()
        {
            ICollection<ReceiverTask> receivers = GetReceivers();
            int itemCount = ItemCount;
            int count = receivers.Count;
            string text = string.Format(CultureInfo.InvariantCulture, "Port Summary:\n    Hash:{0}\n    Type:{1}\n    Elements:{2}\n    ReceiveThunks:{3}\nReceive Arbiter Hierarchy:\n", new object[]
            {
                GetHashCode(),
                base.GetType().GetGenericArguments()[0],
                itemCount,
                count
            });
            ReceiverTask[] receivers2 = GetReceivers();
            for (int i = 0; i < receivers2.Length; i++)
            {
                ReceiverTask receiverTask = receivers2[i];
                string str = receiverTask.ToString();
                text = text + str + "\n";
            }
            return text;
        }

        public override int GetHashCode()
        {
            return Store.Identity;
        }

        public void Clear()
        {
            lock (Store)
            {
                Store.Clear();
            }
        }

        public static implicit operator Receiver<T>(Port<T> port)
        {
            Receiver<T> receiver = new Receiver<T>(false, port, null, (IterativeTask<T>)null)
            {
                KeepItemInPort = true
            };
            return receiver;
        }

        private void PostInternalFast(T item)
        {
            ReceiverTask activeReceiver = Store.ActiveReceiver;
            activeReceiver.Consume(new PortElement<T>(item));
        }

        internal void PostInternal(bool insertAtHead, PortElement<T> node)
        {
            bool flag = false;
            ITask task = null;
            DispatcherQueue dispatcherQueue = null;
            lock (Store)
            {
                if (insertAtHead)
                {
                    Store.ElementListAddFirst(node);
                }
                else
                {
                    Store.ElementListAddLast(node);
                }
                if (Store.ReceiverCount == 0)
                {
                    return;
                }
                int num = 1;
                if (Store.ActiveReceiver == null)
                {
                    num = Store.ReceiverCount;
                }
                ReceiverTask receiverTask = Store.ActiveReceiver;
                int i = 0;
                while (i < num)
                {
                    if (num != 1)
                    {
                        receiverTask = Store.GetReceiverAtIndex(i);
                    }
                    task = null;
                    flag = receiverTask.Evaluate(node, ref task);
                    dispatcherQueue = receiverTask.TaskQueue;
                    if (flag)
                    {
                        Store.ElementListRemove(node);
                        if (receiverTask.State != ReceiverTaskState.Persistent)
                        {
                            Store.RemoveReceiver(receiverTask);
                            if (i > 1)
                            {
                                i--;
                            }
                            else
                            {
                                receiverTask = Store.ActiveReceiver;
                            }
                            num = Store.ReceiverCount;
                            break;
                        }
                        break;
                    }
                    else
                    {
                        if (task != null)
                        {
                            dispatcherQueue.Enqueue(task);
                        }
                        i++;
                    }
                }
            }
            if (flag && task != null)
            {
                dispatcherQueue.Enqueue(task);
            }
        }

        internal bool TestInternal(out PortElement<T> node)
        {
            node = null;
            lock (Store)
            {
                node = Store.ElementListRemoveFirst();
            }
            return node != null;
        }
    }
}