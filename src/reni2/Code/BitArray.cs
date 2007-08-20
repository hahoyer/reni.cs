namespace Reni.Code
{
    /// <summary>
    /// Constant value
    /// </summary>
    public class BitArray : LeafElement
    {
        private readonly Size _size;
        private readonly BitsConst _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BitArray"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="data">The data.</param>
        /// created 23.09.2006 17:52
        public BitArray(Size size, BitsConst data)
        {
            _size = size;
            _data = data;
            StopByObjectId(-1095);
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        /// created 24.09.2006 16:52
        public BitsConst Data { get { return _data; } }
        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        public override Size Size { get { return _size; } }
        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        /// created 23.09.2006 14:23
        public override bool IsEmpty { get { return Data.IsEmpty; } }
       
        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size*(-1); } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            if (Size.IsZero)
                return "";
            return start.BitsArray(Size, Data);
        }

        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        public override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override BitsConst Evaluate()
        {
            return _data.Resize(_size);
        }

        /// <summary>
        /// Creates the void.
        /// </summary>
        /// <returns></returns>
        /// created 08.11.2006 00:37
        public static BitArray CreateVoid()
        {
            return new BitArray(Size.Create(0),BitsConst.None());
        }
    }
}