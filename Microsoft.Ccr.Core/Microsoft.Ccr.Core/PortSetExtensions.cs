using Microsoft.Ccr.Core.Arbiters;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public static class PortSetExtensions
    {
        public static Choice Choice<T0, T1>(this PortSet<T0, T1> portSet)
        {
            return PortSet.ImplicitChoiceOperator(portSet);
        }

        public static Choice Choice<T0, T1, T2>(this PortSet<T0, T1, T2> portSet)
        {
            return PortSet.ImplicitChoiceOperator(portSet);
        }

        public static Choice Choice<T0, T1>(this PortSet<T0, T1> portSet, Handler<T0> handler0, Handler<T1> handler1)
        {
            return new Choice(new ReceiverTask[]
            {
                portSet.P0.Receive(handler0),
                portSet.P1.Receive(handler1)
            });
        }

        public static Choice Choice<T0, T1, T2>(this PortSet<T0, T1, T2> portSet, Handler<T0> handler0, Handler<T1> handler1, Handler<T2> handler2)
        {
            return new Choice(new ReceiverTask[]
            {
                portSet.P0.Receive(handler0),
                portSet.P1.Receive(handler1),
                portSet.P2.Receive(handler2)
            });
        }

        public static MultipleItemGather MultipleItemReceive<T0, T1>(this PortSet<T0, T1> portSet, int totalItemCount, Handler<ICollection<T0>, ICollection<T1>> handler)
        {
            return Arbiter.MultipleItemReceive<T0, T1>(portSet, totalItemCount, handler);
        }

        public static Receiver Receive<T0, T1>(this PortSet<T0, T1> portSet, Handler<T0> handler)
        {
            return new Receiver<T0>(portSet.P0, null, new Task<T0>(handler));
        }

        public static Receiver Receive<T0, T1>(this PortSet<T0, T1> portSet, Handler<T1> handler)
        {
            return new Receiver<T1>(portSet.P1, null, new Task<T1>(handler));
        }
    }
}