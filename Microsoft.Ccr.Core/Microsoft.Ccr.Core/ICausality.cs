using System;

namespace Microsoft.Ccr.Core
{
    public interface ICausality
    {
        Guid Guid
        {
            get;
        }

        string Name
        {
            get;
        }

        IPort ExceptionPort
        {
            get;
        }

        IPort CoordinationPort
        {
            get;
        }
    }
}