using System;

namespace Dorado.Queue
{
    [Serializable]
    public delegate void QueueProcessDelegate(object item);
}