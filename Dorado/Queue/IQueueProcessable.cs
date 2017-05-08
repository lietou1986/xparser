namespace Dorado.Queue
{
    public interface IQueueProcessable
    {
        QueueProcessDelegate ProcessItem
        {
            get;
            set;
        }
    }
}