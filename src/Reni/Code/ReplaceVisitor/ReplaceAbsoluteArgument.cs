namespace Reni.Code.ReplaceVisitor;

sealed class ReplaceAbsoluteArgument : ReplaceArgument
{
    public ReplaceAbsoluteArgument(ResultCache actualArg)
        : base(actualArg) { }

    protected override CodeBase ActualCode => ActualArg.Code;
}