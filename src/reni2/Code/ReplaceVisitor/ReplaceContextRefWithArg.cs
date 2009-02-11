using System;
using HWClassLibrary.Debug;

namespace Reni.Code.ReplaceVisitor
{
    internal abstract class ReplaceContextRef<Context> : Base
        where Context : IRefInCode
    {
        protected readonly Context _context;
        protected readonly CodeBase _replacement;

        protected ReplaceContextRef(Context context, CodeBase replacement)
        {
            _context = context;
            _replacement = replacement;
        }

        internal override CodeBase ContextRef(RefCode visitedObject)
        {
            if(visitedObject.Context == (IRefInCode) _context)
                return _replacement;
            return null;
        }
    }

    /// <summary>
    /// Replaces appearences of context in code tree. 
    /// Assumes, that replacement requires offset alignment when walking along code tree
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    internal sealed class ReplaceRelativeContextRef<Context>: ReplaceContextRef<Context>
        where Context : IRefInCode
    {
        public ReplaceRelativeContextRef(Context context, CodeBase replacement)
            : base(context, replacement) { }

        internal override Visitor<CodeBase> After(Size size)
        {
            return new ReplaceRelativeContextRef<Context>(
                _context,
                _replacement.CreateRefPlus(_context.RefAlignParam, size));
        }
    }

    /// <summary>
    /// Replaces appearences of context in code tree. 
    /// Assumes, that replacement isn't a reference, that changes when walking along the code tree
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    internal sealed class ReplaceAbsoluteContextRef<Context> : ReplaceContextRef<Context>
        where Context : IRefInCode
    {
        public ReplaceAbsoluteContextRef(Context context, CodeBase replacement)
            : base(context, replacement) { }
    }
}