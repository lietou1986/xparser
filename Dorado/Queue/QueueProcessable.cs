using System;

namespace Dorado.Queue
{
    [Serializable]
    public class QueueProcessable : IQueueProcessable
    {
        private object obj;
        private QueueProcessDelegate processItem;

        public object Object
        {
            get
            {
                return this.obj;
            }
            set
            {
                this.obj = value;
            }
        }

        public QueueProcessDelegate ProcessItem
        {
            get
            {
                return this.processItem;
            }
            set
            {
                this.processItem = (QueueProcessDelegate)Delegate.Combine(this.processItem, value);
            }
        }
    }
}