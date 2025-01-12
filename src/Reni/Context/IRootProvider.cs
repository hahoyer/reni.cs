namespace Reni.Context
{
    interface IRootProvider
    {
        [DisableDump]
        Root Value { get; }
    }
}