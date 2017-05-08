using Microsoft.Ccr.Core.Arbiters;

namespace Microsoft.Ccr.Core
{
    public class ConcurrentReceiverGroup
    {
        internal ReceiverTask[] _branches;

        public ConcurrentReceiverGroup(params ReceiverTask[] branches)
        {
            _branches = branches;
        }
    }
}