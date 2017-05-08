using Microsoft.Ccr.Core.Arbiters;
using System;

namespace Microsoft.Ccr.Core
{
    public class TeardownReceiverGroup : InterleaveReceiverGroup
    {
        public TeardownReceiverGroup(params ReceiverTask[] branches) : base(branches)
        {
            if (branches == null)
            {
                throw new ArgumentNullException("branches");
            }
            for (int i = 0; i < branches.Length; i++)
            {
                ReceiverTask receiverTask = branches[i];
                if (receiverTask == null)
                {
                    throw new ArgumentNullException("branches");
                }
                if (receiverTask.State == ReceiverTaskState.Persistent)
                {
                    throw new ArgumentOutOfRangeException("branches", Resource1.TeardownBranchesCannotBePersisted);
                }
            }
        }
    }
}