using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public static class Arbiter
    {
        public static void ExecuteNow(DispatcherQueue dispatcherQueue, ITask task)
        {
            task.TaskQueue = dispatcherQueue;
            TaskExecutionWorker.ExecuteInCurrentThreadContext(task);
        }

        public static void Activate(DispatcherQueue dispatcherQueue, params ITask[] arbiter)
        {
            if (dispatcherQueue == null)
            {
                throw new ArgumentNullException("dispatcherQueue");
            }
            if (arbiter == null)
            {
                throw new ArgumentNullException("arbiter");
            }
            for (int i = 0; i < arbiter.Length; i++)
            {
                ITask task = arbiter[i];
                dispatcherQueue.Enqueue(task);
            }
        }

        public static ITask FromHandler(Handler handler)
        {
            return new Task(handler);
        }

        public static ITask FromIteratorHandler(IteratorHandler handler)
        {
            return new IterativeTask(handler);
        }

        public static ITask ExecuteToCompletion(DispatcherQueue dispatcherQueue, ITask task)
        {
            Port<EmptyValue> done = new Port<EmptyValue>();
            if (task.ArbiterCleanupHandler != null)
            {
                throw new InvalidOperationException(Resource1.TaskAlreadyHasFinalizer);
            }
            task.ArbiterCleanupHandler = delegate
            {
                done.Post(EmptyValue.SharedInstance);
            };
            dispatcherQueue.Enqueue(task);
            return Arbiter.Receive<EmptyValue>(false, done, delegate (EmptyValue e)
            {
            });
        }

        public static void ExecuteToCompletion(DispatcherQueue dispatcherQueue, ITask task, Port<EmptyValue> donePort)
        {
            if (task.ArbiterCleanupHandler != null)
            {
                throw new InvalidOperationException(Resource1.TaskAlreadyHasFinalizer);
            }
            task.ArbiterCleanupHandler = delegate
            {
                donePort.Post(EmptyValue.SharedInstance);
            };
            dispatcherQueue.Enqueue(task);
        }

        public static Receiver<T> Receive<T>(bool persist, Port<T> port, Handler<T> handler)
        {
            return new Receiver<T>(persist, port, null, new Task<T>(handler));
        }

        public static Receiver<T> ReceiveFromPortSet<T>(bool persist, IPortSet portSet, Handler<T> handler)
        {
            return new Receiver<T>(persist, (IPortReceive)portSet[typeof(T)], null, new Task<T>(handler));
        }

        public static Receiver<T> Receive<T>(bool persist, Port<T> port, Handler<T> handler, Predicate<T> predicate)
        {
            return new Receiver<T>(persist, port, predicate, new Task<T>(handler));
        }

        public static Receiver<T> ReceiveFromPortSet<T>(bool persist, IPortSet portSet, Handler<T> handler, Predicate<T> predicate)
        {
            return new Receiver<T>(persist, (IPortReceive)portSet[typeof(T)], predicate, new Task<T>(handler));
        }

        public static Receiver<T> ReceiveWithIterator<T>(bool persist, Port<T> port, IteratorHandler<T> handler)
        {
            return new Receiver<T>(persist, port, null, new IterativeTask<T>(handler));
        }

        public static Receiver<T> ReceiveWithIteratorFromPortSet<T>(bool persist, IPortSet portSet, IteratorHandler<T> handler)
        {
            return new Receiver<T>(persist, (IPortReceive)portSet[typeof(T)], null, new IterativeTask<T>(handler));
        }

        public static Receiver<T> ReceiveWithIterator<T>(bool persist, Port<T> port, IteratorHandler<T> handler, Predicate<T> predicate)
        {
            return new Receiver<T>(persist, port, predicate, new IterativeTask<T>(handler));
        }

        public static Receiver<T> ReceiveWithIteratorFromPortSet<T>(bool persist, IPortSet portSet, IteratorHandler<T> handler, Predicate<T> predicate)
        {
            return new Receiver<T>(persist, (IPortReceive)portSet[typeof(T)], predicate, new IterativeTask<T>(handler));
        }

        public static JoinReceiver JoinedReceive<T0, T1>(bool persist, Port<T0> port0, Port<T1> port1, Handler<T0, T1> handler)
        {
            return new JoinReceiver(persist, new Task<T0, T1>(handler), new IPortReceive[]
            {
                port0,
                port1
            });
        }

        public static JoinReceiver JoinedReceiveWithIterator<T0, T1>(bool persist, Port<T0> port0, Port<T1> port1, IteratorHandler<T0, T1> handler)
        {
            return new JoinReceiver(persist, new IterativeTask<T0, T1>(handler), new IPortReceive[]
            {
                port0,
                port1
            });
        }

        public static JoinSinglePortReceiver MultipleItemReceive<T>(bool persist, Port<T> port, int itemCount, VariableArgumentHandler<T> handler)
        {
            return new JoinSinglePortReceiver(persist, new VariableArgumentTask<T>(itemCount, handler), port, itemCount);
        }

        public static JoinReceiver MultiplePortReceive<T>(bool persist, Port<T>[] ports, VariableArgumentHandler<T> handler)
        {
            return new JoinReceiver(persist, new VariableArgumentTask<T>(ports.Length, handler), (IPortReceive[])ports);
        }

        public static MultipleItemReceiver MultipleItemReceive<T>(VariableArgumentHandler<T> handler, params Port<T>[] ports)
        {
            if (ports == null)
            {
                throw new ArgumentNullException("ports");
            }
            if (ports.Length == 0)
            {
                throw new ArgumentOutOfRangeException("ports");
            }
            return new MultipleItemReceiver(new VariableArgumentTask<T>(ports.Length, handler), (IPortReceive[])ports);
        }

        public static MultipleItemGather MultipleItemReceive<T0, T1>(PortSet<T0, T1> portSet, int totalItemCount, Handler<ICollection<T0>, ICollection<T1>> handler)
        {
            Handler<ICollection[]> handler2 = delegate (ICollection[] res)
            {
                List<T0> list = new List<T0>(res[0].Count);
                List<T1> list2 = new List<T1>(res[1].Count);
                IEnumerator enumerator = res[0].GetEnumerator();

                while (enumerator.MoveNext())
                {
                    T0 item = (T0)((object)enumerator.Current);
                    list.Add(item);
                }

                IEnumerator enumerator2 = res[1].GetEnumerator();

                while (enumerator2.MoveNext())
                {
                    T1 item2 = (T1)((object)enumerator2.Current);
                    list2.Add(item2);
                }

                handler(list, list2);
            };
            return new MultipleItemGather(new Type[]
            {
                typeof(T0),
                typeof(T1)
            }, new IPortReceive[]
            {
                portSet.P0,
                portSet.P1
            }, totalItemCount, handler2);
        }

        public static Interleave Interleave(TeardownReceiverGroup teardown, ExclusiveReceiverGroup exclusive, ConcurrentReceiverGroup concurrent)
        {
            return new Interleave(teardown, exclusive, concurrent);
        }

        public static Choice Choice(params ReceiverTask[] receivers)
        {
            return new Choice(receivers);
        }

        public static Choice Choice(IPortSet portSet)
        {
            return PortSet.ImplicitChoiceOperator(portSet);
        }

        public static Choice Choice<T0, T1>(PortSet<T0, T1> resultPort, Handler<T0> handler0, Handler<T1> handler1)
        {
            return new Choice(new ReceiverTask[]
            {
                resultPort.P0.Receive(handler0),
                resultPort.P1.Receive(handler1)
            });
        }

        public static Port<EmptyValue> WaitForMultipleTasks(DispatcherQueue taskQueue, params ITask[] tasks)
        {
            if (taskQueue == null)
            {
                throw new ArgumentNullException("taskQueue");
            }
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            Port<EmptyValue> port = new Port<EmptyValue>();
            int num = 0;
            for (int i = 0; i < tasks.Length; i++)
            {
                ITask task = tasks[i];
                if (task != null)
                {
                    Arbiter.ExecuteToCompletion(taskQueue, task, port);
                    num++;
                }
            }
            Port<EmptyValue> donePort = new Port<EmptyValue>();
            if (num > 0)
            {
                ITask[] array = new ITask[1];
                array[0] = Arbiter.MultipleItemReceive<EmptyValue>(false, port, num, delegate (EmptyValue[] _)
                {
                    donePort.Post(EmptyValue.SharedInstance);
                });
                Arbiter.Activate(taskQueue, array);
            }
            else
            {
                donePort.Post(EmptyValue.SharedInstance);
            }
            return donePort;
        }
    }
}