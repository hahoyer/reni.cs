using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    sealed class Arg : FiberHead
    {
        static int _nextObjectId;
        readonly TypeBase _type;

        internal Arg(TypeBase type)
            : base(_nextObjectId++)
        {
            _type = type;
            StopByObjectId(-24);
        }

        [Node]
        internal TypeBase Type { get { return _type; } }

        [DisableDump]
        internal override bool IsRelativeReference { get { return _type is IReferenceType; } }

        protected override Size GetSize() { return _type.Size; }
        protected override CodeArgs GetRefsImplementation() { return CodeArgs.Arg(); }
        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Arg(this); }
    }
}