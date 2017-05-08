namespace Microsoft.Ccr.Core
{
    public enum TaskExecutionPolicy
    {
        Unconstrained,
        ConstrainQueueDepthDiscardTasks,
        ConstrainQueueDepthThrottleExecution,
        ConstrainSchedulingRateDiscardTasks,
        ConstrainSchedulingRateThrottleExecution
    }
}