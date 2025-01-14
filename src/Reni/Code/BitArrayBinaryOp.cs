using Reni.Basics;

namespace Reni.Code;

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
        StopByObjectIds(-3);
    }

    protected override FiberItem[]? TryToCombineImplementation(FiberItem subsequentElement)
        => subsequentElement.TryToCombineBack(this);

    [DisableDump]
    internal override Size OutputSize { get; }

    internal override void Visit(IVisitor visitor) => visitor.BitArrayBinaryOp(OpToken, OutputSize, LeftSize, RightSize);

    protected override string GetNodeDump() => base.GetNodeDump() + " <" + LeftSize + "> " + OpToken + " <" + RightSize + ">";

    protected override TFiber? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TFiber : default
        => actual.BitArrayBinaryOp(this);
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

    protected override FiberItem[]? TryToCombineImplementation(FiberItem subsequentElement)
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
        StopByObjectIds(-10);
    }

    [DisableDump]
    internal override Size OutputSize => Size.Zero;

    protected override string GetNodeDump() => base.GetNodeDump() + " <" + LeftSize + "> dump_print <" + RightSize + ">";

    internal override void Visit(IVisitor visitor) => visitor.PrintNumber(LeftSize, RightSize);

    protected override TFiber? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TFiber : default
        => actual.DumpPrintNumberOperation(this);
}

sealed class DumpPrintTextOperation : FiberItem
{
    internal readonly Size ItemSize;
    internal DumpPrintTextOperation(Size leftSize, Size itemSize)
    {
        (leftSize != null).Assert();
        (itemSize != null).Assert();
        InputSize = leftSize;
        ItemSize = itemSize;
    }

    internal override Size InputSize { get; }
    [DisableDump]
    internal override Size OutputSize => Size.Zero;
    protected override string GetNodeDump() => base.GetNodeDump() + " <" + InputSize + "> dump_print_text(" + ItemSize + ")";
    internal override void Visit(IVisitor visitor) => visitor.PrintText(InputSize, ItemSize);

    protected override TFiber? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TFiber : default
        => actual.DumpPrintTextOperation(this);
}

sealed class DumpPrintText : FiberHead
{
    [Node]
    [EnableDump]
    internal readonly string Value;

    internal DumpPrintText(string value) { Value = value; }

    protected override Size GetSize() => Size.Zero;
    internal override void Visit(IVisitor visitor) => visitor.PrintText(Value);

    protected override string GetNodeDump() => base.GetNodeDump() + " dump_print " + Value.Quote();

    protected override TCode? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TCode : default
        => actual.DumpPrintText(this);
}