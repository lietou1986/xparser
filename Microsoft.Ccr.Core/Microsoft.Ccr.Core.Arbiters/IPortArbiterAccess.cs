namespace Microsoft.Ccr.Core.Arbiters
{
    public interface IPortArbiterAccess
    {
        PortMode Mode
        {
            get;
            set;
        }

        IPortElement TestForElement();

        IPortElement[] TestForMultipleElements(int count);

        void PostElement(IPortElement element);
    }
}