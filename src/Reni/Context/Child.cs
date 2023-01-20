using hw.DebugFormatter;
using hw.Helper;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Context;

abstract class Child : ContextBase
{
    [DisableDump]
    readonly ContextBase Parent;

    internal Child(ContextBase parent)
    {
        Parent = parent.AssertNotNull();
        Parent.AssertIsNotNull();
    }

    protected abstract string GetContextChildIdentificationDump();

    public sealed override string GetContextIdentificationDump()
        => Parent.GetContextIdentificationDump() + GetContextChildIdentificationDump();

    [DisableDump]
    internal override bool IsRecursionMode => Parent.IsRecursionMode;

    [DisableDump]
    internal override Root RootContext => Parent.RootContext;

    internal override CompoundView GetRecentCompoundView()
        => Parent.GetRecentCompoundView();

    internal override IFunctionContext GetRecentFunctionContext()
        => Parent.GetRecentFunctionContext();

    internal override IEnumerable<IImplementation> GetDeclarations<TDefinable>
        (TDefinable tokenClass)
    {
        var result = base.GetDeclarations(tokenClass).ToArray();
        return result.Any()? result : Parent.GetDeclarations(tokenClass);
    }

    [DisableDump]
    internal override IEnumerable<ContextBase> ParentChain
        => Parent.ParentChain.Concat(base.ParentChain);
}