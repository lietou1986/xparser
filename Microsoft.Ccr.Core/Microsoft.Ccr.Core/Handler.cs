namespace Microsoft.Ccr.Core
{
    public delegate void Handler();

    public delegate void Handler<T0>(T0 parameter0);

    public delegate void Handler<T0, T1>(T0 parameter0, T1 parameter1);

    public delegate void Handler<T0, T1, T2>(T0 parameter0, T1 parameter1, T2 parameter2);
}