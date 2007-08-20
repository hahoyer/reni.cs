using System;
using Reni.Code;
using Reni.Context;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Replace context elements
    /// </summary>
    abstract public class ReplaceContextRef<CC> : Base where CC : Context.Base
    {
        /// <summary>
        /// the context
        /// </summary>
        protected readonly CC _context;
        /// <summary>
        /// the code to use as replacement
        /// </summary>
        protected readonly Code.Base _replacement;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceStructContextRef"/> class.
        /// </summary>
        /// <param name="context">The struct container.</param>
        /// <param name="replacement">The replacement.</param>
        protected ReplaceContextRef(CC context, Code.Base replacement)
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
        public override Code.Base ContextRef<C>(ContextRef<C> visitedObject)
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
    public sealed class ReplaceRelativeContextRef<CC> : ReplaceContextRef<CC> where CC : Context.Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceRelativeContextRef&lt;CC&gt;"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// created 04.01.2007 18:18
        public ReplaceRelativeContextRef(CC context, Code.Base replacement)
            : base(context, replacement)
        {
        }

        /// <summary>
        /// Afters the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// created 15.10.2006 18:32
        public override Visitor<Code.Base> After(Size size)
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
    public sealed class ReplaceAbsoluteContextRef<CC> : ReplaceContextRef<CC> where CC : Context.Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceRelativeContextRef&lt;CC&gt;"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="replacement">The replacement.</param>
        /// created 04.01.2007 18:18
        public ReplaceAbsoluteContextRef(CC context, Code.Base replacement)
            : base(context, replacement)
        {
        }
    }
}