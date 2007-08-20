using HWClassLibrary.Debug;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    /// <summary>
    /// Bit array operation
    /// </summary>
    internal sealed class BitArrayOp : BinaryOp
    {
        private readonly Defineable _opToken;
        private readonly Size _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitArrayOp"/> class.
        /// </summary>
        /// <param name="opToken">The op token.</param>
        /// <param name="size">The size.</param>
        /// <param name="leftSize">Size of the left.</param>
        /// <param name="rightSize">Size of the right.</param>
        /// created 05.10.2006 23:38
        internal BitArrayOp(Defineable opToken, Size size, Size leftSize, Size rightSize) 
            : base(leftSize,rightSize)
        {
            _opToken = opToken;
            _size = size;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// created 29.09.2006 03:18
        internal Defineable OpToken { get { return _opToken; } }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 23.09.2006 14:15
        /// created 29.09.2006 03:18
        public override Size Size { get { return _size; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.BitArrayOp(OpToken, Size, LeftSize, RightSize);
        }

        /// <summary>
        /// Tries to combine two leaf elements. .
        /// </summary>
        /// <param name="subsequentElement">the element that follows.</param>
        /// <returns>null if no combination possible (default) or a leaf element that contains the combination of both</returns>
        /// created 19.10.2006 21:18
        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }
    }

    /// <summary>
    /// Bit array prafix operation
    /// </summary>
    sealed internal class BitArrayPrefixOp : LeafElement
    {
        private readonly Defineable _opToken;
        private readonly Size _size;

        internal BitArrayPrefixOp(Defineable name, Size size)
        {
            _opToken = name;
            _size = size;
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return _size; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size.Zero; } }
        /// <summary>
        /// Gets the op token.
        /// </summary>
        /// <value>The op token.</value>
        /// created 02.02.2007 23:59
        public Defineable OpToken { get { return _opToken; } }
        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.BitArrayPrefixOp(OpToken, Size);
        }

    }
    /// <summary>
    /// Dump and print
    /// </summary>
    sealed internal class DumpPrint : BinaryOp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DumpPrint"/> class.
        /// </summary>
        /// <param name="leftSize">Size of the left.</param>
        /// <param name="rightSize">Size of the right.</param>
        /// created 08.01.2007 16:57
        /// created 08.01.2007 16:58
        internal DumpPrint(Size leftSize, Size rightSize) : base(leftSize, rightSize)
        {
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        public override Size Size { get { return Size.Zero; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.DumpPrint(LeftSize, RightSize);
        }

    }

    sealed internal class DumpPrintText : LeafElement
    {
        [DumpData(true)]
        private readonly string _dumpPrintText;

        internal DumpPrintText(string dumpPrintText)
        {
            _dumpPrintText = dumpPrintText;
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        /// created 05.10.2006 23:40
        [DumpData(false)]
        public override Size Size { get { return Size.Zero; } }

        /// <summary>
        /// Gets the size of the delta.
        /// </summary>
        /// <value>The size of the delta.</value>
        /// created 10.10.2006 00:21
        public override Size DeltaSize { get { return Size.Zero; } }

        /// <summary>
        /// Formats this instance.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// created 07.10.2006 21:11
        protected override string Format(StorageDescriptor start)
        {
            return start.DumpPrintText(_dumpPrintText);
        }
    }
}