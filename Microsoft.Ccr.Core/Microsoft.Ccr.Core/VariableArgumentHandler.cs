namespace Microsoft.Ccr.Core
{
    public delegate void VariableArgumentHandler<T>(params T[] t);

    public delegate void VariableArgumentHandler<T0, T>(T0 t0, params T[] t);
}