using Reni.Basics;

namespace Reni.Code;

sealed class EmptyStackData : StackData
{
    internal EmptyStackData(IOutStream? outStream)
        : base(outStream) { }

    internal override StackData Push(StackData stackData) => stackData;
    internal override StackData PushOnto(NonListStackData formerStack) => formerStack;
    internal override StackData PushOnto(ListStack formerStack) => formerStack;
    internal override Size Size => Size.Zero;
    internal override BitsConst GetBitsConst() => BitsConst.GetNone();
}