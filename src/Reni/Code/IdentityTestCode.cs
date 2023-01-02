using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code;

sealed class IdentityTestCode : FiberItem
{
    [EnableDump]
    readonly bool IsEqual;

    [EnableDump]
    readonly Size Size;

    internal IdentityTestCode(bool isEqual, Size size)
    {
        IsEqual = isEqual;
        Size = size;
    }

    [DisableDump]
    internal override Size InputSize => Size * 2;

    [DisableDump]
    internal override Size OutputSize => Size.Bit;

    internal override void Visit(IVisitor visitor)
    {
        NotImplementedMethod(visitor);
        return;
    }
}