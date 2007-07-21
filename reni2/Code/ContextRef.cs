namespace Reni.Code
{
    /// <summary>
    /// Context reference, should be replaced
    /// </summary>
    public sealed class ContextRef<C> : Base where C: Context.Base
    {
        private readonly C _context;

        /// <summary>
        /// Initializes a new instance of the ContextRefCode class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ContextRef(C context)
        {
            _context = context;
            StopByObjectId(496);
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public C Context { get{ return _context;} }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size { get { return Context.RefAlignParam.RefSize; } }

        public override Refs Refs { get { return Reni.Refs.Context(_context); } }

        /// <summary>
        /// Visitor to replace parts of code, overridable version.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <returns></returns>
        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.ContextRef(this);
        }

    }
}