using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;

namespace Reni.Sequence
{
    sealed class Star
        : Operation<Star>
    {
        protected override int Signature(int objSize, int argSize) { return BitsConst.MultiplySize(objSize, argSize); }
    }
}