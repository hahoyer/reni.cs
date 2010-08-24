using System;
using HWClassLibrary.Debug;

namespace Reni.Code.ReplaceVisitor
{
    internal abstract class ReplaceContextRef<Context> : Base
        where Context : IReferenceInCode
    {
        protected readonly Context _context;
        protected readonly Func<CodeBase> _replacement;

        protected ReplaceContextRef(Context context, Func<CodeBase> replacement)
        {
            _context = context;
            _replacement = replacement;
        }

        internal override CodeBase ContextRef(ReferenceCode visitedObject)
        {
            if(visitedObject.Context == (IReferenceInCode) _context)
                return _replacement();
            return null;
        }
    }

    /// <summary>
    /// Replaces appearences of refInCode in code tree. 
    /// Assumes, that replacement requires offset alignment when walking along code tree
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    internal sealed class ReplaceRelativeContextRef<Context>: ReplaceContextRef<Context>
        where Context : IReferenceInCode
    {
        public ReplaceRelativeContextRef(Context context, Func<CodeBase> replacement)
            : base(context, replacement) { }

        protected override Visitor<CodeBase> After(Size size)
        {
            return new ReplaceRelativeContextRef<Context>(
                _context,
                ()=>_replacement().AddToReference(_context.RefAlignParam, size, "After"));
        }
    }

    /// <summary>
    /// Replaces appearences of refInCode in code tree. 
    /// Assumes, that replacement isn't a reference, that changes when walking along the code tree
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    internal sealed class ReplaceAbsoluteContextRef<Context> : ReplaceContextRef<Context>
        where Context : IReferenceInCode
    {
        public ReplaceAbsoluteContextRef(Context context, Func<CodeBase> replacement)
            : base(context, replacement) { }
    }
}