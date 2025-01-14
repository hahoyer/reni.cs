using Reni.Basics;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Code;

sealed class IdentityTestCode : FiberItem
{
    [EnableDump]
    internal readonly bool IsEqual;

    [EnableDump]
    internal readonly Size ArgumentSize;

    [DisableDump]
    internal override Size OutputSize { get; }

    string DataFunctionName => EqualityOperation.TokenId(IsEqual).Symbolize();

    internal IdentityTestCode(bool isEqual, Size outputSize, Size size)
    {
        OutputSize = outputSize;
        IsEqual = isEqual;
        ArgumentSize = size;
    }

    [DisableDump]
    internal override Size InputSize => ArgumentSize * 2;

    internal override void Visit(IVisitor visitor)
        => visitor.BitArrayBinaryOp(DataFunctionName, OutputSize, ArgumentSize, ArgumentSize);

    protected override FiberItem[]? TryToCombineImplementation(FiberItem subsequentElement)
        => subsequentElement.TryToCombineBack(this);
}