using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Ccr.Core
{
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    [Serializable]
    public class PortNotFoundException : Exception
    {
        private readonly IPort _port;

        private readonly object _objectPosted;

        public IPort Port
        {
            get
            {
                return _port;
            }
        }

        public object ObjectPosted
        {
            get
            {
                return _objectPosted;
            }
        }

        public PortNotFoundException()
        {
        }

        protected PortNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PortNotFoundException(string message) : base(message)
        {
        }

        public PortNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PortNotFoundException(IPort port, object posted, string message) : base(message)
        {
            if (port == null)
            {
                throw new ArgumentNullException("port");
            }
            if (posted == null)
            {
                throw new ArgumentNullException("posted");
            }
            _port = port;
            _objectPosted = posted;
        }

        public PortNotFoundException(IPort port, object posted) : this(port, posted, (posted != null) ? ("Type not expected: " + posted.GetType().FullName) : "Unknown type not expected")
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}