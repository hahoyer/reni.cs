using System;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    abstract class ReplaceContextRef<TContext> : Base
        where TContext : IContextReference
    {
        static int _nextObjectId;
        protected readonly TContext Context;
        protected readonly Func<CodeBase> Replacement;

        protected ReplaceContextRef(TContext context, Func<CodeBase> replacement)
            : base(_nextObjectId++)
        {
            Context = context;
            Replacement = replacement;
        }

        internal override CodeBase ContextRef(ReferenceCode visitedObject)
        {
            if(visitedObject.Target == (IContextReference) Context)
                return Replacement();
            return null;
        }
    }

    sealed class ReplaceRelativeContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IContextReference
    {
        public ReplaceRelativeContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) {}

        protected override Visitor<CodeBase, FiberItem> After(Size size)
        {
            return new ReplaceRelativeContextRef<TContext>(Context, () => AfterCode(size));
        }

        CodeBase AfterCode(Size size) => Replacement().ReferencePlus(size);
    }

    sealed class ReplaceAbsoluteContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IContextReference
    {
        public ReplaceAbsoluteContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) {}
    }
}