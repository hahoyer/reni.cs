using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.Helper;
using Reni.Parser.TokenClass;

namespace Reni.Code
{
    /// <summary>
    /// Bit array operation
    /// </summary>
    [Serializable]
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
            return start.CreateBitArrayOp(OpToken, GetSize(), LeftSize, RightSize);
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
    [Serializable]
    internal sealed class BitArrayPrefixOp : LeafElement
    {
        [Node]
        internal readonly Defineable OpToken;
        private readonly Size _size;
        [Node]
        internal readonly Size ArgSize;

        internal BitArrayPrefixOp(Defineable name, Size size, Size argSize)
        {
            OpToken = name;
            _size = size;
            ArgSize = argSize;
        }

        protected override Size GetSize()
        {
            return _size;
        }

        protected override Size GetInputSize()
        {
            return ArgSize;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateBitArrayPrefixOp(OpToken, GetSize(), ArgSize);
        }
        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBackN(this);
        }

        public override string NodeDump { get { return base.NodeDump + " " + OpToken.Name + " " + ArgSize; } }
    }

    /// <summary>
    /// Dump and print
    /// </summary>
    [Serializable]
    internal sealed class DumpPrint : BinaryOp
    {
        internal DumpPrint(Size leftSize, Size rightSize) : base(leftSize, rightSize) {}

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.CreateDumpPrint(LeftSize, RightSize);
        }
        public override string NodeDump { get { return base.NodeDump + " <" + LeftSize + "> dump_print <" + RightSize + ">"; } }
    }
    [Serializable]

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

        protected override Size GetInputSize()
        {
            return Size.Zero;
        }

        protected override string Format(StorageDescriptor start)
        {
            return StorageDescriptor.CreateDumpPrintText(_dumpPrintText);
        }

        public override string NodeDump { get { return base.NodeDump + " dump_print " + HWString.Quote(_dumpPrintText); } }
    }
}