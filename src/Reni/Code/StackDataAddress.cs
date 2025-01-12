using Reni.Basics;

namespace Reni.Code;

sealed class StackDataAddress : NonListStackData
{
    internal sealed class GetTopException : Exception
    {
        public GetTopException(Size size)
            : base("GetTop failed for size = " + size.Dump()) { }
    }

    internal sealed class PullException : Exception
    {
        public PullException(Size size)
            : base("Pull failed for size = " + size.Dump()) { }
    }

    [EnableDump]
    readonly IStackDataAddressBase Data;

    [EnableDump]
    readonly Size Offset;

    public StackDataAddress(IStackDataAddressBase data, Size offset, IOutStream outStream)
        : base(outStream)
    {
        Data = data;
        Offset = offset;
    }

    protected override StackData GetTop(Size size) => throw new GetTopException(size);
    protected override StackData Pull(Size size) => throw new PullException(size);

    internal override Size Size => DataStack.RefSize;

    internal override StackData BitArrayBinaryOp(string opToken, Size size, StackData right)
    {
        if(size == DataStack.RefSize && opToken == "Plus")
            return RefPlus(Size.Create(right.GetBitsConst().ToInt32()));


        NotImplementedMethod(opToken, size, right);
        return null;
    }

    protected override StackDataAddress GetAddress() => this;

    protected override string Dump(bool isRecursion)
    {
        if(isRecursion)
            throw new NotImplementedException();
        return Data.Dump() + "[" + Offset.ToInt() + "]";
    }

    internal new StackData Dereference(Size size, Size dataSize)
        => Data.GetTop(Offset, size).BitCast(dataSize).BitCast(size);

    internal new void Assign(Size size, StackData right)
        => Data.SetTop(Offset, right.Dereference(size, size));

    internal new StackData RefPlus(Size offset)
    {
        if(offset.IsZero)
            return this;
        return new StackDataAddress(Data, offset + Offset, OutStream);
    }
}

interface IStackDataAddressBase
{
    StackData GetTop(Size offset, Size size);
    string Dump();
    void SetTop(Size offset, StackData right);
}