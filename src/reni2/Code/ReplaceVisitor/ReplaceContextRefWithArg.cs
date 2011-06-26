using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    internal abstract class ReplaceContextRef<TContext> : Base
        where TContext : IReferenceInCode
    {
        private static int _nextObjectId;
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
            if(visitedObject.Context == (IReferenceInCode) Context)
                return Replacement();
            return null;
        }
    }

    internal sealed class ReplaceRelativeContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IReferenceInCode
    {
        public ReplaceRelativeContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) { }

        protected override Visitor<CodeBase> After(Size size) { return new ReplaceRelativeContextRef<TContext>(Context, () => AfterCode(size)); }

        private CodeBase AfterCode(Size size) { return Replacement().AddToReference(Context.RefAlignParam, size); }
    }

    internal sealed class ReplaceAbsoluteContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IReferenceInCode
    {
        public ReplaceAbsoluteContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) { }
    }
}