using Microsoft.Ccr.Core.Arbiters;

namespace Microsoft.Ccr.Core
{
    public class InterleaveReceiverGroup
    {
        internal ReceiverTask[] _branches;

        public InterleaveReceiverGroup(params ReceiverTask[] branches)
        {
            _branches = branches;
        }
    }
}