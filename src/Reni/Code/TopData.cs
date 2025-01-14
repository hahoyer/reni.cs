using Reni.Basics;

namespace Reni.Code;

sealed class TopData : Top
{
    public TopData(Size offset, Size size, Size dataSize)
        : base(offset, size, dataSize)
        => StopByObjectIds(-110);

    protected override CodeBase? TryToCombine(FiberItem subsequentElement)
        => subsequentElement.TryToCombineBack(this);

    internal override void Visit(IVisitor visitor) => visitor.TopData(Offset, Size, DataSize);

    protected override TCode? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TCode : default
        => actual.TopData(this);
}

sealed class TopFrameData : Top
{
    public TopFrameData(Size offset, Size size, Size dataSize)
        : base(offset, size, dataSize)
        => StopByObjectIds();

    protected override CodeBase? TryToCombine(FiberItem subsequentElement)
        => subsequentElement.TryToCombineBack(this);

    internal override void Visit(IVisitor visitor)
        => visitor.TopFrameData(Offset, Size, DataSize);

    protected override TCode? VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
        where TCode : default
        => actual.TopFrameData(this);
}