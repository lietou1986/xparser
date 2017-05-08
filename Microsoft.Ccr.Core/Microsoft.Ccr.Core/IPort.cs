namespace Microsoft.Ccr.Core
{
    public interface IPort
    {
        void PostUnknownType(object item);

        bool TryPostUnknownType(object item);
    }
}