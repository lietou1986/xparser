using System;

namespace Microsoft.Ccr.Core
{
    [Flags]
    public enum DispatcherOptions
    {
        None = 0,
        UseBackgroundThreads = 1,
        UseProcessorAffinity = 2,
        SuppressDisposeExceptions = 4,
        UseHighAccuracyTimerLogic = 8
    }
}