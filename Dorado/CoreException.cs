using System;
using System.Runtime.Serialization;

namespace Dorado
{
    [Serializable()]
    public class CoreException : ApplicationException
    {
        public CoreException()
            : base()
        {
        }

        public CoreException(string message)
            : base(message)
        {
        }

        public CoreException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }

        public CoreException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CoreException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}