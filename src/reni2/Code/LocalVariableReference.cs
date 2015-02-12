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
        internal readonly Holder Holder;
        [Node]
        [DisableDump]
        internal readonly Size Offset;

        public LocalVariableReference(Holder holder, Size offset)
            : base(_nextObjectId++)
        {
            Holder = holder;
            Offset = offset ?? Size.Zero;
            StopByObjectId(-2);
        }

        [DisableDump]
        [Node]
        internal override bool IsRelativeReference => true;

        protected override string GetNodeDump() => base.GetNodeDump()
            + " Holder=" + Holder
            + " Offset=" + Offset;

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        internal override void Visit(IVisitor visitor) => visitor.LocalVariableReference(Holder.Name, Offset);
        protected override CodeBase TryToCombine(FiberItem subsequentElement) => subsequentElement.TryToCombineBack(this);
    }
}