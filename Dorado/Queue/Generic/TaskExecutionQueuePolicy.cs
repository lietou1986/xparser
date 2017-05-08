namespace Dorado.Queue.Generic
{
    public enum TaskExecutionQueuePolicy
    {
        ConstrainQueueDepthDiscardTasks = 1,
        ConstrainQueueDepthThrottleExecution
    }
}