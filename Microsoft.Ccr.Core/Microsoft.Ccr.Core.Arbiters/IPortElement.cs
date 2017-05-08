namespace Microsoft.Ccr.Core.Arbiters
{
    public interface IPortElement
    {
        IPort Owner
        {
            get;
            set;
        }

        IPortElement Next
        {
            get;
            set;
        }

        IPortElement Previous
        {
            get;
            set;
        }

        object CausalityContext
        {
            get;
            set;
        }

        object Item
        {
            get;
        }
    }

    public interface IPortElement<T> : IPortElement
    {
        T TypedItem
        {
            get;
        }
    }
}