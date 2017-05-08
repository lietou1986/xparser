using Microsoft.Ccr.Core.Arbiters;

namespace Microsoft.Ccr.Core
{
    public interface IPortReceive
    {
        int ItemCount
        {
            get;
        }

        object Test();

        void RegisterReceiver(ReceiverTask receiver);

        void UnregisterReceiver(ReceiverTask receiver);

        ReceiverTask[] GetReceivers();

        object[] GetItems();

        void Clear();
    }
}