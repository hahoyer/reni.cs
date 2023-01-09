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

    internal override CompoundView ObtainRecentCompoundView()
        => Parent.ObtainRecentCompoundView();

    internal override IFunctionContext ObtainRecentFunctionContext()
        => Parent.ObtainRecentFunctionContext();

    internal override IEnumerable<IImplementation> Declarations<TDefinable>
        (TDefinable tokenClass)
    {
        var result = base.Declarations(tokenClass).ToArray();
        return result.Any()? result : Parent.Declarations(tokenClass);
    }

    [DisableDump]
    internal override IEnumerable<ContextBase> ParentChain
        => Parent.ParentChain.Concat(base.ParentChain);
}