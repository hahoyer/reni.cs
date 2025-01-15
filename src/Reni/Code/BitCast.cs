using Reni.Basics;

namespace Reni.Code;

/// <summary>
///     Expression to change size of an expression
/// </summary>
sealed class BitCast : FiberItem
{
    static int NextId;

    [Node]
    [DisableDump]
    internal readonly Size InputDataSize;

    [DisableDump]
    internal override Size OutputSize { get; }

    [DisableDump]
    internal override Size InputSize { get; }

    internal BitCast(Size outputSize, Size inputSize, Size inputDataSize)
        : base(NextId++)
    {
        (outputSize != inputSize || inputSize != inputDataSize).Assert();
        OutputSize = outputSize;
        InputSize = inputSize;
        InputDataSize = inputDataSize;
        StopByObjectIds();
    }

    protected override FiberItem[]? TryToCombineImplementation(FiberItem subsequentElement)
        => subsequentElement.TryToCombineBack(this);

    internal override FiberItem[] TryToCombineBack(BitCast preceding)
    {
        var inputDataSize = InputDataSize.GetMin(preceding.InputDataSize);
        if(OutputSize == InputSize && OutputSize == inputDataSize)
            return [];
        if(OutputSize == preceding.InputSize && OutputSize == inputDataSize)
            return [];
        return [new BitCast(OutputSize, preceding.InputSize, inputDataSize)];
    }

    internal override CodeBase TryToCombineBack(BitArray precedingElement)
    {
        var bitsConst = precedingElement.Data;
        if(bitsConst.Size > InputDataSize)
            bitsConst = bitsConst.Resize(InputDataSize);
        return new BitArray(OutputSize, bitsConst);
    }

    protected override string GetNodeDump()
        => base.GetNodeDump() + " InputSize=" + InputSize + " InputDataSize=" + InputDataSize;

    internal override void Visit(IVisitor visitor)
        => visitor.BitCast(OutputSize, InputSize, InputDataSize);

    internal override CodeBase? TryToCombineBack(TopData precedingElement)
    {
        if(precedingElement.Size == InputSize
           && OutputSize >= InputDataSize
           && OutputSize > InputSize)
        {
            var result = new TopData(precedingElement.Offset, OutputSize, InputDataSize);
            return result
                .Concat(new BitCast(OutputSize, OutputSize, InputDataSize));
        }

        return null;
    }

    internal override CodeBase? TryToCombineBack(TopFrameData precedingElement)
    {
        if(precedingElement.Size == InputSize
           && OutputSize >= InputDataSize
           && OutputSize > InputSize)
            return new TopFrameData(precedingElement.Offset, OutputSize, InputDataSize)
                .Concat(new BitCast(OutputSize, OutputSize, InputDataSize));
        return null;
    }

    internal override FiberItem[]? TryToCombineBack(BitArrayBinaryOp precedingElement)
    {
        if(InputSize == OutputSize)
            return null;

        FiberItem bitArrayOp = new BitArrayBinaryOp
        (
            precedingElement.OpToken,
            precedingElement.OutputSize + OutputSize - InputSize,
            precedingElement.LeftSize,
            precedingElement.RightSize);

        if(InputDataSize == OutputSize)
            return [bitArrayOp];

        return [bitArrayOp, new BitCast(OutputSize, OutputSize, InputDataSize)];
    }

    internal override FiberItem[]? TryToCombineBack(IdentityTestCode precedingElement)
    {
        if(InputSize == OutputSize)
            return null;

        FiberItem bitArrayOp = new IdentityTestCode
        (
            precedingElement.IsEqual,
            precedingElement.OutputSize + OutputSize - InputSize,
            precedingElement.ArgumentSize);

        if(InputDataSize == OutputSize)
            return [bitArrayOp];

        return [bitArrayOp, new BitCast(OutputSize, OutputSize, InputDataSize)];
    }

    internal override FiberItem[]? TryToCombineBack(BitArrayPrefixOp precedingElement)
    {
        if(InputSize == OutputSize)
            return null;

        var bitArrayOp = new BitArrayPrefixOp
        (
            precedingElement.Operation,
            precedingElement.OutputSize + OutputSize - InputSize,
            precedingElement.ArgSize);

        if(InputDataSize == OutputSize)
            return [bitArrayOp];

        return [bitArrayOp, new BitCast(OutputSize, OutputSize, InputDataSize)];
    }

    internal override FiberItem[]? TryToCombineBack(DePointer preceding)
    {
        if(InputSize == OutputSize && InputDataSize <= preceding.DataSize)
        {
            var dereference = new DePointer(OutputSize, InputDataSize);
            return [dereference];
        }

        if(InputSize < OutputSize)
        {
            var dereference = new DePointer(OutputSize, preceding.DataSize);
            if(OutputSize == InputDataSize)
                return [dereference];
            return
            [
                dereference, new BitCast(OutputSize, OutputSize, InputDataSize)
            ];
        }

        return null;
    }

    protected override TFiber? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TFiber : default
        => actual.BitCast(this);
}