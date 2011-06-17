using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.Sequence
{
    internal sealed class Star : SequenceOfBitOperation
    {
        protected override int ResultSize(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
    }
}