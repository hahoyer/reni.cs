using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class LocalVariableReference : FiberHead
    {
        static int _nextObjectId;
        [Node]
        [DisableDump]
        internal readonly string Holder;
        [Node]
        [DisableDump]
        internal readonly Size Offset;

        public LocalVariableReference(string holder, Size offset)
            : base(_nextObjectId++)
        {
            Holder = holder;
            Offset = offset ?? Size.Zero;
            StopByObjectId(-2);
        }

        [DisableDump]
        [Node]
        internal override bool IsRelativeReference { get { return true; } }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump()
                + " Holder=" + Holder
                + " Offset=" + Offset;
        }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }
        internal override void Visit(IVisitor visitor) { visitor.LocalVariableReference(Holder, Offset); }
        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}