using System;
using HWClassLibrary.Debug;

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

    /// <summary>
    /// Replaces appearences of refInCode in code tree. 
    /// Assumes, that replacement requires offset alignment when walking along code tree
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    internal sealed class ReplaceRelativeContextRef<TContext>: ReplaceContextRef<TContext>
        where TContext : IReferenceInCode
    {
        public ReplaceRelativeContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) { }

        protected override Visitor<CodeBase> After(Size size)
        {
            return new ReplaceRelativeContextRef<TContext>(
                Context,
                ()=>Replacement().AddToReference(Context.RefAlignParam, size, "After"));
        }
    }

    /// <summary>
    /// Replaces appearences of refInCode in code tree. 
    /// Assumes, that replacement isn't a reference, that changes when walking along the code tree
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    internal sealed class ReplaceAbsoluteContextRef<TContext> : ReplaceContextRef<TContext>
        where TContext : IReferenceInCode
    {
        public ReplaceAbsoluteContextRef(TContext context, Func<CodeBase> replacement)
            : base(context, replacement) { }
    }
}