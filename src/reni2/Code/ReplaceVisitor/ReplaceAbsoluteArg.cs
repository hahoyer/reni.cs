namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Handle argument replaces of absolute args
    /// </summary>
    internal sealed class ReplaceAbsoluteArg : ReplaceArg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReplaceAbsoluteArg"/> class.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// created 28.09.2006 22:46
        public ReplaceAbsoluteArg(Code.CodeBase actual)
            : base(actual)
        {
        }

        /// <summary>
        /// Gets the actual.
        /// </summary>
        /// <value>The actual.</value>
        /// created 28.09.2006 22:46
        public override Code.CodeBase Actual { get { return ActualArg; } }

    }
}