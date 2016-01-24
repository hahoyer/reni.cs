using System;
using System.Collections.Generic;
using System.Linq;
using hw.Forms;
using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    sealed class Arg : FiberHead
    {
        static int _nextObjectId;

        internal Arg(TypeBase type)
            : base(_nextObjectId++)
        {
            Type = type;
            StopByObjectIds();
        }

        [Node]
        internal TypeBase Type { get; }

        protected override Size GetSize() => Type.Size;
        protected override CodeArgs GetRefsImplementation() => CodeArgs.Arg();
        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual) => actual.Arg(this);
    }
}