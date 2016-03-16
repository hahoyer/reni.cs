using System.Collections.Generic;
using System.Linq;
using System;
using hw.DebugFormatter;

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
            StopByObjectIds(-945);
        }

        protected override Size GetSize() => _size;

        [DisableDump]
        internal override bool IsRelativeReference => true;

        [Node]
        [DisableDump]
        protected internal Size DataSize => _dataSize;

        protected override string GetNodeDump() => base.GetNodeDump() + " Offset=" + Offset + " DataSize=" + _dataSize;
    }
}