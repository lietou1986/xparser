using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    public delegate IEnumerator<ITask> IteratorHandler();

    public delegate IEnumerator<ITask> IteratorHandler<T0>(T0 parameter0);

    public delegate IEnumerator<ITask> IteratorHandler<T0, T1>(T0 parameter0, T1 parameter1);

    public delegate IEnumerator<ITask> IteratorHandler<T0, T1, T2>(T0 parameter0, T1 parameter1, T2 parameter2);
}