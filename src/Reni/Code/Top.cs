using Reni.Basics;

namespace Reni.Code;

abstract class Top : FiberHead
{
    [Node]
    [DisableDump]
    internal readonly Size Offset;

    readonly Size SizeValue;
    readonly Size DataSizeValue;

    protected Top(Size offset, Size size, Size dataSize)
    {
        Offset = offset;
        SizeValue = size;
        DataSizeValue = dataSize;
        StopByObjectIds(-945);
    }

    protected override Size GetSize() => SizeValue;

    [DisableDump]
    internal override bool IsRelativeReference => true;

    [Node]
    [DisableDump]
    protected internal Size DataSize => DataSizeValue;

    protected override string GetNodeDump() => base.GetNodeDump() + " Offset=" + Offset + " DataSize=" + DataSizeValue;
}