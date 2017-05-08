using System;
using System.Runtime.Serialization;

namespace Dorado.Platform.Exceptions
{
    [Serializable]
    public class ResumeParseException : Exception
    {
        public ResumeParseException(string message) : base(message)
        {
        }

        public ResumeParseException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ResumeParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}