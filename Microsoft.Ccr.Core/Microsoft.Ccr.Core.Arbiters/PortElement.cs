namespace Microsoft.Ccr.Core.Arbiters
{
    public class PortElement<T> : IPortElement<T>, IPortElement
    {
        private Port<T> _Owner;

        internal PortElement<T> _next;

        internal PortElement<T> _previous;

        internal object _causalityContext;

        internal T _item;

        public IPort Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                _Owner = (Port<T>)value;
            }
        }

        public IPortElement Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = (PortElement<T>)value;
            }
        }

        public IPortElement Previous
        {
            get
            {
                return _previous;
            }
            set
            {
                _previous = (PortElement<T>)value;
            }
        }

        public object CausalityContext
        {
            get
            {
                return _causalityContext;
            }
            set
            {
                _causalityContext = value;
            }
        }

        public object Item
        {
            get
            {
                return _item;
            }
        }

        public T TypedItem
        {
            get
            {
                return _item;
            }
            internal set
            {
                _item = value;
            }
        }

        public PortElement(T item)
        {
            _item = item;
        }

        public PortElement(T item, Port<T> owner)
        {
            _Owner = owner;
            _item = item;
        }
    }
}