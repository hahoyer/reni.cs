using System;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using HWClassLibrary.Helper;
using Reni.Context;
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
        [Node, IsDumpEnabled(false)]
        internal readonly ISequenceOfBitBinaryOperation OpToken;
        private readonly Size _size;

        internal BitArrayBinaryOp(ISequenceOfBitBinaryOperation opToken, Size size, Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            OpToken = opToken;
            _size = size;
            StopByObjectId(-381);
        }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _size; } }

        internal override string CSharpString() { return CSharpGenerator.BitArrayBinaryOp(OpToken, OutputSize, LeftSize, RightSize); }

        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.BitArrayBinaryOp(OpToken, OutputSize, LeftSize, RightSize); }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " <" + LeftSize + "> " + OpToken.DataFunctionName + " <" + RightSize + ">"; } }
    }

    /// <summary>
    /// Bit array prefix operation
    /// </summary>
    [Serializable]
    internal sealed class BitArrayPrefixOp : FiberItem
    {
        [Node]
        [IsDumpEnabled(false)]
        internal readonly ISequenceOfBitPrefixOperation OpToken;
        private readonly Size _size;
        [Node]
        [IsDumpEnabled(false)]
        internal readonly Size ArgSize;

        internal BitArrayPrefixOp(ISequenceOfBitPrefixOperation name, Size size, Size argSize)
        {
            OpToken = name;
            _size = size;
            ArgSize = argSize;
        }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return ArgSize; } }
        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _size; } }
        
        internal override void Execute(IFormalMaschine formalMaschine)
        {
            NotImplementedMethod(formalMaschine);
            throw new NotImplementedException();
        }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override string CSharpString() { return CSharpGenerator.BitArrayPrefix(OpToken, OutputSize); }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " " + OpToken.DataFunctionName + " " + ArgSize; } }
    }

    /// <summary>
    /// Dump and print
    /// </summary>
    [Serializable]
    internal sealed class DumpPrintOperation : BinaryOp
    {
        internal DumpPrintOperation(Size leftSize, Size rightSize) : base(leftSize, rightSize) {}

        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return Size.Zero; } }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " <" + LeftSize + "> dump_print <" + RightSize + ">"; } }
        internal override string CSharpString() { return CSharpGenerator.DumpPrint(); }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.DumpPrintOperation(LeftSize,RightSize); }
    }
    [Serializable]

    internal sealed class DumpPrintText : FiberHead
    {
        [Node, IsDumpEnabled(true)]
        private readonly string _dumpPrintText;

        internal DumpPrintText(string dumpPrintText)
        {
            _dumpPrintText = dumpPrintText;
        }

        protected override Size GetSize() { return Size.Zero; }
        protected override string CSharpString() { return CSharpGenerator.DumpPrintText(_dumpPrintText); }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.DumpPrintText(); }
        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " dump_print " + _dumpPrintText.Quote(); } }
    }
}