namespace Reni.Code;

interface IContextReference
{
    int Order { get; }
}

interface IContextReferenceProvider
{
    IContextReference ContextReference { get; }
}