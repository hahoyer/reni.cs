using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Replace context elements
    /// </summary>
    internal abstract class ReplaceContextRef<Context> : Base
        where Context : ContextBase
    {
        protected readonly Context _context;

        protected readonly CodeBase _replacement;

        protected ReplaceContextRef(Context context, CodeBase replacement)
        {
            _context = context;
            _replacement = replacement;
        }

        internal override CodeBase ContextRef<C>(ContextRef<C> visitedObject)
        {
            if(_context != visitedObject.Context)
                return null;
            return _replacement;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    internal sealed class ReplaceRelativeContextRef<Context> : ReplaceContextRef<Context>
        where Context : ContextBase
    {
        public ReplaceRelativeContextRef(Context context, CodeBase replacement)
            : base(context, replacement) { }

        internal override Visitor<CodeBase> After(Size size)
        {
            if(size.IsZero)
                return this;
            return new ReplaceRelativeContextRef<Context>(_context, _replacement.CreateRefPlus(_context.RefAlignParam, size));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Context"></typeparam>
    internal sealed class ReplaceAbsoluteContextRef<Context> : ReplaceContextRef<Context>
        where Context : ContextBase
    {
        public ReplaceAbsoluteContextRef(Context context, CodeBase replacement)
            : base(context, replacement) { }
    }
}