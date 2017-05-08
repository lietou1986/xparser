namespace Microsoft.Ccr.Core.Arbiters
{
    public interface IArbiterTask : ITask
    {
        ArbiterTaskState ArbiterState
        {
            get;
        }

        bool Evaluate(ReceiverTask receiver, ref ITask deferredTask);
    }
}