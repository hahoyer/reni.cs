namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Replace context elements
    /// </summary>
    internal abstract class ReplaceContextRef<CC> : Base where CC : Context.ContextBase
    {
        /// <summary>
        /// the context
        /// </summary>
        protected readonly CC _context;
        /// <summary>
        /// the code to use as replacement
        /// </summary>
        protected readonly Code.CodeBase _replacement;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceContextRef&lt;CC&gt;"/> class.
        /// </summary>
        /// <param name="context">The struct container.</param>
        /// <param name="replacement">The replacement.</param>
        protected ReplaceContextRef(CC context, Code.CodeBase replacement)
        {
            _context = context;
            _replacement = replacement;
        }

        /// <summary>
        /// Contexts the ref.
        /// </summary>
        /// <param name="visitedObject">The visited object.</param>
        /// <returns></returns>
        /// created 17.10.2006 00:04
        internal override Code.CodeBase ContextRef<C>(ContextRef<C> visitedObject)
        {
            if (_context != visitedObject.Context)
                return null;
            return _replacement;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="CC"></typeparam>
    internal sealed class ReplaceRelativeContextRef<CC> : ReplaceContextRef<CC> where CC : Context.ContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceRelativeContextRef&lt;CC&gt;"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// created 04.01.2007 18:18
        public ReplaceRelativeContextRef(CC context, Code.CodeBase replacement)
            : base(context, replacement)
        {
        }

        /// <summary>
        /// Afters the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 15.10.2006 18:32
        internal override Visitor<Code.CodeBase> After(Size size)
        {
            if (size.IsZero)
                return this;
            return new ReplaceRelativeContextRef<CC>(_context, _replacement.CreateRefPlus(_context.RefAlignParam, size));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="CC"></typeparam>
    internal sealed class ReplaceAbsoluteContextRef<CC> : ReplaceContextRef<CC> where CC : Context.ContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceRelativeContextRef&lt;CC&gt;"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// created 04.01.2007 18:18
        public ReplaceAbsoluteContextRef(CC context, Code.CodeBase replacement)
            : base(context, replacement)
        {
        }
    }
}