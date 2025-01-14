using Reni.Basics;

namespace Reni.Code.ReplaceVisitor;

abstract class ReplaceContextReference<TContext> : Base
    where TContext : IContextReference
{
    static int NextObjectId;
    protected readonly TContext Context;
    protected readonly Func<CodeBase> Replacement;

    protected ReplaceContextReference(TContext context, Func<CodeBase> replacement)
        : base(NextObjectId++)
    {
        Context = context;
        Replacement = replacement;
    }

    internal override CodeBase? ContextReference(ReferenceCode visitedObject)
    {
        if(visitedObject.Target == (IContextReference)Context)
            return Replacement();
        return null;
    }
}

sealed class ReplaceRelativeContextReference<TContext> : ReplaceContextReference<TContext>
    where TContext : IContextReference
{
    public ReplaceRelativeContextReference(TContext context, Func<CodeBase> replacement)
        : base(context, replacement) { }

    protected override Visitor<CodeBase, FiberItem> After
        (Size size) => new ReplaceRelativeContextReference<TContext>(Context, () => AfterCode(size));

    CodeBase AfterCode(Size size) => Replacement().GetReferenceWithOffset(size);
}

sealed class ReplaceAbsoluteContextReference<TContext> : ReplaceContextReference<TContext>
    where TContext : IContextReference
{
    public ReplaceAbsoluteContextReference(TContext context, Func<CodeBase> replacement)
        : base(context, replacement) { }
}