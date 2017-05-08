using Microsoft.Ccr.Core.Arbiters;

namespace Microsoft.Ccr.Core
{
    internal class MultipleItemHelperReceiver : Receiver
    {
        private MultipleItemReceiver _parent;

        public MultipleItemHelperReceiver(IPortReceive port, MultipleItemReceiver parent) : base(false, port, null)
        {
            _parent = parent;
        }

        public override bool Evaluate(IPortElement messageNode, ref ITask deferredTask)
        {
            return _parent.Evaluate((int)_arbiterContext, messageNode, ref deferredTask);
        }
    }
}