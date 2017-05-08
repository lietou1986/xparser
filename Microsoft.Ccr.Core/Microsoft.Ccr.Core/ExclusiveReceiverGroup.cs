using Microsoft.Ccr.Core.Arbiters;

namespace Microsoft.Ccr.Core
{
    public class ExclusiveReceiverGroup
    {
        internal ReceiverTask[] _branches;

        public ExclusiveReceiverGroup(params ReceiverTask[] branches)
        {
            _branches = branches;
        }
    }
}