using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.Helper;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    /// Bit array operation
    /// </summary>
    [Serializable]
    internal sealed class BitArrayBinaryOp : BinaryOp
    {
        [Node, DumpData(false)]
        internal readonly ISequenceOfBitBinaryOperation OpToken;
        private readonly Size _size;

        internal BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            OpToken = opToken;
            _size = size;
            StopByObjectId(-381);
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

        public override string NodeDump { get { return base.NodeDump + " <"+LeftSize+"> "+
            OpToken.DataFunctionName+" <"+RightSize+">"; } }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitArrayBinaryOp(OpToken, Size, LeftSize, RightSize); }
    }

    /// <summary>
    /// Bit array prefix operation
    /// </summary>
    [Serializable]
    internal sealed class BitArrayPrefixOp : LeafElement
    {
        [Node]
        internal readonly ISequenceOfBitPrefixOperation OpToken;
        private readonly Size _size;
        [Node]
        internal readonly Size ArgSize;

        internal BitArrayPrefixOp(ISequenceOfBitPrefixOperation name, Size size, Size argSize)
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

        public override string NodeDump { get { return base.NodeDump + " " + OpToken.DataFunctionName + " " + ArgSize; } }
    }

    /// <summary>
    /// Dump and print
    /// </summary>
    [Serializable]
    internal sealed class DumpPrintOperation : BinaryOp
    {
        internal DumpPrintOperation(Size leftSize, Size rightSize) : base(leftSize, rightSize) {}

        protected override Size GetSize()
        {
            return Size.Zero;
        }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.DumpPrintOperation(LeftSize,RightSize); }

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

        public override string NodeDump { get { return base.NodeDump + " dump_print " + _dumpPrintText.Quote(); } }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.DumpPrintText(); }
    }
}