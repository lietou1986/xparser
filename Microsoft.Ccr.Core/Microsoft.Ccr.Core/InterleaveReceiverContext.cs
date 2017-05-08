using Microsoft.Ccr.Core.Arbiters;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    internal class InterleaveReceiverContext
    {
        public InterleaveReceivers ReceiverGroup;

        public Queue<Tuple<ITask, ReceiverTask>> PendingItems = new Queue<Tuple<ITask, ReceiverTask>>();

        public InterleaveReceiverContext(InterleaveReceivers receiverGroup)
        {
            ReceiverGroup = receiverGroup;
        }
    }
}