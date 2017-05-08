using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    internal sealed class Store<T>
    {
        internal int Identity;

        private List<ReceiverTask> Receivers;

        internal ReceiverTask ActiveReceiver;

        private PortElement<T> Elements;

        internal int ElementCount;

        internal ReceiverTask[] ReceiverListAsObjectArray
        {
            get
            {
                if (ActiveReceiver != null)
                {
                    return new ReceiverTask[]
                    {
                        ActiveReceiver
                    };
                }
                ReceiverTask[] array = new ReceiverTask[Receivers.Count];
                Receivers.CopyTo(array, 0);
                return array;
            }
        }

        internal int ReceiverCount
        {
            get
            {
                if (ActiveReceiver != null)
                {
                    return 1;
                }
                if (Receivers == null)
                {
                    return 0;
                }
                return Receivers.Count;
            }
        }

        internal bool IsElementListEmpty
        {
            get
            {
                return Elements == null;
            }
        }

        internal PortElement<T> ElementListFirst
        {
            get
            {
                return Elements;
            }
        }

        internal object[] ElementListAsObjectArray
        {
            get
            {
                if (IsElementListEmpty)
                {
                    return new object[0];
                }
                List<IPortElement> list = new List<IPortElement>();
                IPortElement portElement = ElementListFirst;
                do
                {
                    list.Add(portElement);
                    portElement = portElement.Next;
                }
                while (portElement != Elements);
                return list.ToArray();
            }
        }

        internal void AddReceiver(ReceiverTask r)
        {
            if (ActiveReceiver == null && (Receivers == null || Receivers.Count == 0))
            {
                ActiveReceiver = r;
                return;
            }
            if (Receivers == null)
            {
                Receivers = new List<ReceiverTask>();
            }
            if (ActiveReceiver != null)
            {
                Receivers.Add(ActiveReceiver);
                ActiveReceiver = null;
            }
            Receivers.Add(r);
        }

        internal void RemoveReceiver(ReceiverTask r)
        {
            if (ActiveReceiver == r)
            {
                ActiveReceiver = null;
                return;
            }
            if (Receivers == null)
            {
                return;
            }
            Receivers.Remove(r);
            if (Receivers.Count == 1)
            {
                ActiveReceiver = Receivers[0];
                Receivers.Clear();
            }
        }

        internal ReceiverTask GetReceiverAtIndex(int i)
        {
            if (ActiveReceiver == null)
            {
                return Receivers[i];
            }
            if (i > 0)
            {
                throw new ArgumentOutOfRangeException("i");
            }
            return ActiveReceiver;
        }

        internal void ElementListAddFirst(PortElement<T> Item)
        {
            if (Elements == null)
            {
                Elements = Item;
                Item._next = Item;
                Item._previous = Item;
                ElementCount++;
                return;
            }
            if (Elements._next == Elements)
            {
                PortElement<T> elements = Elements;
                Elements = Item;
                Elements._next = elements;
                Elements._previous = elements;
                elements._next = Elements;
                elements._previous = Elements;
                ElementCount++;
                return;
            }
            PortElement<T> elements2 = Elements;
            Elements = Item;
            Item._next = elements2;
            Item._previous = elements2._previous;
            elements2._previous._next = Item;
            elements2._previous = Item;
            ElementCount++;
        }

        internal void ElementListAddLast(PortElement<T> Item)
        {
            if (Elements == null)
            {
                Elements = Item;
                Item._next = Item;
                Item._previous = Item;
            }
            else
            {
                Elements._previous._next = Item;
                Item._previous = Elements._previous;
                Item._next = Elements;
                Elements._previous = Item;
            }
            ElementCount++;
        }

        internal PortElement<T> ElementListRemoveFirst()
        {
            if (Elements == null)
            {
                return null;
            }
            if (Elements._next == Elements)
            {
                PortElement<T> elements = Elements;
                Elements = null;
                ElementCount--;
                return elements;
            }
            PortElement<T> elements2 = Elements;
            Elements = Elements._next;
            Elements._previous = elements2._previous;
            Elements._previous._next = Elements;
            ElementCount--;
            return elements2;
        }

        internal void ElementListRemove(PortElement<T> Item)
        {
            ElementCount--;
            if (Item == Elements)
            {
                if (ElementCount == 0)
                {
                    Elements = null;
                    return;
                }
                Elements = Item._next;
            }
            Item._previous._next = Item._next;
            Item._next._previous = Item._previous;
        }

        internal void Clear()
        {
            ElementCount = 0;
            Elements = null;
        }
    }
}