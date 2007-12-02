namespace Reni.Type
{
    /// <summary>
    /// Type of type
    /// </summary>
    internal sealed class TypeType: Primitive
    {
        /// <summary>
        /// Converts to itself.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 30.01.2007 22:57
        internal override Result ConvertToItself(Category category)
        {
            return CreateVoidResult(category);
        }

        private readonly Base _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeType"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// created 08.01.2007 18:05
        public TypeType(Base parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// The size of type
        /// </summary>
        public override Size Size { get { return Size.Zero; } }

        /// <summary>
        /// Gets the dump print text.
        /// </summary>
        /// <value>The dump print text.</value>
        /// created 08.01.2007 17:54
        internal override string DumpPrintText { get { return "("+_parent.DumpPrintText+"()) type"; } }

        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        internal override Result DumpPrint(Category category)
        {
            return Void.CreateResult
                (
                category,
                delegate { return DumpPrintCode(); }
                );
        }
        /// <summary>
        /// Dumps the print code.
        /// </summary>
        /// <returns></returns>
        /// created 08.01.2007 17:29
        Code.Base DumpPrintCode()
        {
            return Code.Base.CreateDumpPrintText(_parent.DumpPrintText);
        }
    }
}