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
        internal readonly LocalVariableDefinition Definition;
        [Node]
        [DisableDump]
        internal readonly Size Offset;

        public LocalVariableReference(LocalVariableDefinition definition, Size offset)
            : base(_nextObjectId++)
        {
            Definition = definition;
            Offset = offset ?? Size.Zero;
            StopByObjectId(-2);
        }

        [DisableDump]
        [Node]
        internal override bool IsRelativeReference => true;

        protected override string GetNodeDump() => base.GetNodeDump()
            + " " + nameof(Definition.NameInCode) + "=" + Definition.NameInCode
            + " Offset=" + Offset;

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        internal override void Visit(IVisitor visitor) => visitor.LocalVariableReference(Definition.NameInCode, Offset);
        protected override CodeBase TryToCombine(FiberItem subsequentElement) => subsequentElement.TryToCombineBack(this);
    }
}