using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Sequence
{
    internal sealed class Slash : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.DivideSize(objSize, argSize); }
    }
}