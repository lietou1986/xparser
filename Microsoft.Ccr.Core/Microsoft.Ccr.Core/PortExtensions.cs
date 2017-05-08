using System;

namespace Microsoft.Ccr.Core
{
    public static class PortExtensions
    {
        public static Receiver Receive<T>(this Port<T> port)
        {
            return port;
        }

        public static Receiver Receive<T>(this Port<T> port, Handler<T> handler)
        {
            return new Receiver<T>(port, null, new Task<T>(handler));
        }

        public static Receiver Receive<T>(this Port<T> port, Handler<T> handler, Predicate<T> predicate)
        {
            return new Receiver<T>(port, predicate, new Task<T>(handler));
        }

        public static JoinReceiver Join<T0, T1>(this Port<T0> port, Port<T1> port1, Handler<T0, T1> handler)
        {
            return new JoinReceiver(false, new Task<T0, T1>(handler), new IPortReceive[]
            {
                port,
                port1
            });
        }

        public static JoinReceiver Join<T0, T1, T2>(this Port<T0> port, Port<T1> port1, Port<T2> port2, Handler<T0, T1, T2> handler)
        {
            return new JoinReceiver(false, new Task<T0, T1, T2>(handler), new IPortReceive[]
            {
                port,
                port1,
                port2
            });
        }

        public static JoinSinglePortReceiver Join<T0>(this Port<T0> port, int itemCount, VariableArgumentHandler<T0> handler)
        {
            return new JoinSinglePortReceiver(false, new VariableArgumentTask<T0>(itemCount, handler), port, itemCount);
        }
    }
}