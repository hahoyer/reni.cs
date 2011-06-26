using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

namespace Reni.Code
{
    internal sealed class EmptyStackData : StackData
    {
        internal override StackData Push(StackData stackData) { return stackData; }
        internal override StackData PushOnto(NonListStackData formerStack) { return formerStack; }
        internal override StackData PushOnto(ListStack formerStack) { return formerStack; }
        internal override Size Size { get { return Size.Zero; } }
        internal override BitsConst GetBitsConst() { return BitsConst.None(); }
    }
}