using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal abstract class Top : FiberHead
    {
        [Node, IsDumpEnabled(false)]
        protected internal readonly Size Offset;

        private readonly RefAlignParam _refAlignParam;
        private readonly Size _size;
        private readonly Size _dataSize;

        protected Top(RefAlignParam refAlignParam, Size offset, Size size, Size dataSize)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
            _size = size;
            _dataSize = dataSize;
            StopByObjectId(-945);
        }

        protected override Size GetSize() { return _size; }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [IsDumpEnabled(false)]
        protected Size DataSize { get { return _dataSize; } }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset + " DataSize=" + _dataSize; } }
    }
}