using HWClassLibrary.Debug;

namespace Reni.Code
{
    /// <summary>
    /// Code for end of statement
    /// </summary>
    public sealed class StatementEnd : LeafElement
    {
        private readonly Size _intermediateSize;
        private readonly Size _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:StatementEnd"/> class.
        /// </summary>
        /// <param name="intermediateSize">The intermediateSize.</param>
        /// <param name="size">The size.</param>
        /// created 02.10.2006 17:56
        public StatementEnd(Size size, Size intermediateSize)
        {
            Tracer.Assert(!intermediateSize.IsZero);

            _intermediateSize = intermediateSize;
            _size = size;
            StopByObjectId(-219);
        }

        /// <summary>
        /// Gets the body.
        /// </summary>
        /// <value>The body.</value>
        /// created 29.09.2006 03:55
        public Size IntermediateSize { get { return _intermediateSize; } }
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size { get { return _size; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return IntermediateSize; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.StatementEnd(Size, IntermediateSize);
        }

    }
}