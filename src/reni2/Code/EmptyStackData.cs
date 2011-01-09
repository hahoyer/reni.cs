using System;

namespace Reni.Code
{
    internal sealed class EmptyStackData : StackData
    {
        internal override StackData Push(StackData stackData) { return stackData; }
        internal override Size Size { get { return Size.Zero; } }
        internal override BitsConst GetBitsConst() { return BitsConst.None(); }
    }
}