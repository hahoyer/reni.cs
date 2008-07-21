using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    /// <summary>
    /// Bit array operation
    /// </summary>
    internal sealed class BitArrayOp : BinaryOp
    {
        [Node]
        internal readonly Defineable OpToken;
        private readonly Size _size;

        internal BitArrayOp(Defineable opToken, Size size, Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            OpToken = opToken;
            _size = size;
        }

        protected override Size GetSize()
        {
            return _size;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.BitArrayOp(OpToken, GetSize(), LeftSize, RightSize);
        }

        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBackN(this);
        }

        public override string NodeDump { get { return base.NodeDump + " <"+LeftSize+"> "+OpToken.Name+" <"+RightSize+">"; } }
    }

    /// <summary>
    /// Bit array prafix operation
    /// </summary>
    internal sealed class BitArrayPrefixOp : LeafElement
    {
        private readonly Defineable OpToken;
        private readonly Size _size;

        internal BitArrayPrefixOp(Defineable name, Size size)
        {
            OpToken = name;
            _size = size;
        }

        protected override Size GetSize()
        {
            return _size;
        }

        protected override Size GetDeltaSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.BitArrayPrefixOp(OpToken, GetSize());
        }

        public override string NodeDump { get { return base.NodeDump + " " + OpToken.Name; } }
    }

    /// <summary>
    /// Dump and print
    /// </summary>
    internal sealed class DumpPrint : BinaryOp
    {
        internal DumpPrint(Size leftSize, Size rightSize) : base(leftSize, rightSize) {}

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.DumpPrint(LeftSize, RightSize);
        }
        public override string NodeDump { get { return base.NodeDump + " <" + LeftSize + "> dump_print <" + RightSize + ">"; } }
    }

    internal sealed class DumpPrintText : LeafElement
    {
        [Node, DumpData(true)]
        private readonly string _dumpPrintText;

        internal DumpPrintText(string dumpPrintText)
        {
            _dumpPrintText = dumpPrintText;
        }

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override Size GetDeltaSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.DumpPrintText(_dumpPrintText);
        }

        public override string NodeDump { get { return base.NodeDump + " dump_print " + HWString.Quote(_dumpPrintText); } }
    }
}