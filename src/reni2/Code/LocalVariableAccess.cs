using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    sealed class LocalVariableAccess : FiberHead
    {
        static int _nextObjectId;
        [DisableDump]
        [Node]
        internal readonly LocalVariableDefinition Definition;
        [DisableDump]
        [Node]
        internal readonly Size Offset;
        readonly Size _size;
        [DisableDump]
        [Node]
        internal readonly Size DataSize;

        public LocalVariableAccess
            (LocalVariableDefinition definition, Size offset, Size size, Size dataSize)
            : base(_nextObjectId++)
        {
            Definition = definition;
            Offset = offset;
            _size = size;
            DataSize = dataSize;
            StopByObjectId(-1);
            StopByObjectId(-7);
        }

        protected override string GetNodeDump() => base.GetNodeDump()
            + " " + nameof(Definition.NameInCode) + "=" + Definition.NameInCode
            + " Offset=" + Offset
            + " DataSize=" + DataSize;

        protected override Size GetSize() => _size;
        internal override void Visit(IVisitor visitor)
            => visitor.LocalVariableAccess(Definition.NameInCode, Offset, Size, DataSize);
        protected override CodeBase TryToCombine(FiberItem subsequentElement)
            => subsequentElement.TryToCombineBack(this);
    }
}