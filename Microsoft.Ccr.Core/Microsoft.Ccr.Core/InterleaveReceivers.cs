using System;

namespace Microsoft.Ccr.Core
{
    [Flags]
    public enum InterleaveReceivers
    {
        Teardown = 1,
        Exclusive = 2,
        Concurrent = 4
    }
}