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
        internal readonly string Holder;
        [DisableDump]
        [Node]
        internal readonly Size Offset;
        readonly Size _size;
        [DisableDump]
        [Node]
        internal readonly Size DataSize;

        public LocalVariableAccess(string holder, Size offset, Size size, Size dataSize)
            : base(_nextObjectId++)
        {
            Holder = holder;
            Offset = offset;
            _size = size;
            DataSize = dataSize;
            StopByObjectId(-1);
            StopByObjectId(-7);
        }

        protected override string GetNodeDump()
        {
            return base.GetNodeDump()
                + " Holder=" + Holder
                + " Offset=" + Offset
                + " DataSize=" + DataSize
                ;
        }

        protected override Size GetSize() { return _size; }
        internal override void Visit(IVisitor visitor) { visitor.LocalVariableAccess(Holder, Offset, Size, DataSize); }
        protected override CodeBase TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }
    }
}