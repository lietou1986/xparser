using Microsoft.Ccr.Core.Arbiters;
using System;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public interface IPortSet : IPort
    {
        ICollection<IPort> Ports
        {
            get;
        }

        IPort this[Type portItemType]
        {
            get;
        }

        Port<object> SharedPort
        {
            get;
        }

        PortSetMode Mode
        {
            get;
            set;
        }

        T Test<T>();
    }
}