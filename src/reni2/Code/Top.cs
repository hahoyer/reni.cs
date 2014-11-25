using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    abstract class Top : FiberHead
    {
        [Node]
        [DisableDump]
        internal readonly Size Offset;

        readonly Size _size;
        readonly Size _dataSize;

        protected Top(Size offset, Size size, Size dataSize)
        {
            Offset = offset;
            _size = size;
            _dataSize = dataSize;
            StopByObjectId(-945);
        }

        protected override Size GetSize() { return _size; }

        [DisableDump]
        internal override bool IsRelativeReference { get { return true; } }

        [Node]
        [DisableDump]
        protected Size DataSize { get { return _dataSize; } }

        protected override string GetNodeDump() { return base.GetNodeDump() + " Offset=" + Offset + " DataSize=" + _dataSize; }
    }
}