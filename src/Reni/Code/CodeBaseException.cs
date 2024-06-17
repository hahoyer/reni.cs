namespace Reni.Code;

abstract class CodeBaseException : Exception
{
    [Node]
    readonly IContextReference Container;

    protected CodeBaseException(IContextReference container) => Container = container;
    public override string Message => Container.ToString();
}

sealed class UnexpectedContextReference : CodeBaseException
{
    internal UnexpectedContextReference(IContextReference container)
        : base(container) { }
}

sealed class UnexpectedRecursiveCallCandidate : Exception;