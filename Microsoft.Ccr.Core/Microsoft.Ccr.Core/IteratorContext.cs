using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    internal class IteratorContext
    {
        internal IEnumerator<ITask> _iterator;

        internal CausalityThreadContext _causalities;

        public IteratorContext(IEnumerator<ITask> iterator, CausalityThreadContext causalities)
        {
            _iterator = iterator;
            _causalities = causalities;
        }
    }
}