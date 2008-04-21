namespace Reni.Code
{
    /// <summary>
    /// Binary operations
    /// </summary>
    internal abstract class BinaryOp : LeafElement
    {
        readonly Size _leftSize;
        readonly Size _rightSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BinaryOp"/> class.
        /// </summary>
        /// <param name="leftSize">Size of the left.</param>
        /// <param name="rightSize">Size of the right.</param>
        /// created 08.01.2007 16:57
        public BinaryOp(Size leftSize, Size rightSize)
        {
            _leftSize = leftSize;
            _rightSize = rightSize;
        }

        /// <summary>
        /// Gets the size of the right.
        /// </summary>
        /// <value>The size of the right.</value>
        /// created 04.10.2006 00:13
        public Size RightSize { get { return _rightSize; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return LeftSize + RightSize - Size; } }

        /// <summary>
        /// Gets the size of the left.
        /// </summary>
        /// <value>The size of the left.</value>
        /// created 29.09.2006 03:18
        public Size LeftSize { get { return _leftSize; } }
    }
}