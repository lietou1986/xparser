using Microsoft.Ccr.Core.Arbiters;

namespace Microsoft.Ccr.Core
{
    internal class GatherPrivateReceiver : Receiver
    {
        private MultipleItemGather _parent;

        public GatherPrivateReceiver(IPortReceive port, MultipleItemGather parent) : base(true, port, null)
        {
            _parent = parent;
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            return _parent.Evaluate(messageNode.Item, ref deferredTask);
        }
    }
}