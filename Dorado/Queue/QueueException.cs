using System;

namespace Dorado.Queue
{
    [Serializable]
    public class QueueException : Exception
    {
        public QueueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public QueueException(string message)
            : base(message)
        {
        }
    }
}