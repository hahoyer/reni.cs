using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code;

sealed class BitArray : FiberHead
{
    [Node]
    [DisableDump]
    internal readonly BitsConst Data;

    readonly Size SizeValue;

    internal new static BitArray Void => new(Size.Create(0), BitsConst.GetNone());

    public BitArray(Size size, BitsConst data)
    {
        //Tracer.Assert(size.IsPositive);
        SizeValue = size;
        Data = data;
        StopByObjectIds();
    }

    public BitArray()
        : this(Size.Zero, BitsConst.GetNone()) { }

    protected override Size GetSize() => SizeValue;

    internal override IEnumerable<CodeBase> ToList()
    {
        if(IsHollow)
            return new CodeBase[0];
        return base.ToList();
    }

    protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        => actual.BitArray(this);

    [DisableDump]
    internal override bool IsEmpty => IsHollow;

    protected override CodeBase TryToCombine(FiberItem subsequentElement)
        => subsequentElement.TryToCombineBack(this);

    internal override void Visit(IVisitor visitor) => visitor.BitsArray(Size, Data);

    protected override string GetNodeDump() => base.GetNodeDump() + " Data=" + Data;
}