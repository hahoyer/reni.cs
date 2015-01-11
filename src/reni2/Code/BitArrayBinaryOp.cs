using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Bit array operation
    /// </summary>
    sealed class BitArrayBinaryOp : BinaryOp
    {
        [Node]
        [DisableDump]
        internal readonly string OpToken;

        internal BitArrayBinaryOp(string opToken, Size size, Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            OpToken = opToken;
            OutputSize = size;
            StopByObjectId(-3);
        }

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        [DisableDump]
        internal override Size OutputSize { get; }

        internal override void Visit(IVisitor visitor) => visitor.BitArrayBinaryOp(OpToken, OutputSize, LeftSize, RightSize);

        protected override string GetNodeDump() => base.GetNodeDump() + " <" + LeftSize + "> " + OpToken + " <" + RightSize + ">";
    }

    /// <summary>
    ///     Bit array prefix operation
    /// </summary>
    sealed class BitArrayPrefixOp : FiberItem
    {
        [Node]
        [DisableDump]
        internal readonly string Operation;

        [Node]
        [DisableDump]
        internal readonly Size ArgSize;

        internal BitArrayPrefixOp(string operation, Size size, Size argSize)
        {
            OutputSize = size;
            ArgSize = argSize;
            Operation = operation;
        }

        [DisableDump]
        internal override Size InputSize => ArgSize;

        [DisableDump]
        internal override Size OutputSize { get; }

        internal override void Visit(IVisitor visitor) => visitor.BitArrayPrefixOp(Operation, OutputSize, ArgSize);

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);

        protected override string GetNodeDump() => base.GetNodeDump() + " " + Operation + " " + ArgSize;
    }

    /// <summary>
    ///     Dump and print
    /// </summary>
    sealed class DumpPrintNumberOperation : BinaryOp
    {
        internal DumpPrintNumberOperation(Size leftSize, Size rightSize)
            : base(leftSize, rightSize)
        {
            StopByObjectId(-10);
        }

        [DisableDump]
        internal override Size OutputSize => Size.Zero;

        protected override string GetNodeDump() => base.GetNodeDump() + " <" + LeftSize + "> dump_print <" + RightSize + ">";

        internal override void Visit(IVisitor visitor) => visitor.PrintNumber(LeftSize, RightSize);
    }

    sealed class DumpPrintTextOperation : FiberItem
    {
        readonly Size _itemSize;
        internal DumpPrintTextOperation(Size leftSize, Size itemSize)
        {
            InputSize = leftSize;
            _itemSize = itemSize;
        }

        internal override Size InputSize { get; }
        [DisableDump]
        internal override Size OutputSize => Size.Zero;
        protected override string GetNodeDump() => base.GetNodeDump() + " <" + InputSize + "> dump_print_text(" + _itemSize + ")";
        internal override void Visit(IVisitor visitor) => visitor.PrintText(InputSize, _itemSize);
    }

    sealed class DumpPrintText : FiberHead
    {
        [Node]
        [EnableDump]
        readonly string _dumpPrintText;

        internal DumpPrintText(string dumpPrintText) { _dumpPrintText = dumpPrintText; }

        protected override Size GetSize() => Size.Zero;
        internal override void Visit(IVisitor visitor) => visitor.PrintText(_dumpPrintText);

        protected override string GetNodeDump() => base.GetNodeDump() + " dump_print " + _dumpPrintText.Quote();
    }
}