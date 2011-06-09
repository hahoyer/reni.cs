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
        [Node, DisableDump]
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

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DisableDump]
        protected Size DataSize { get { return _dataSize; } }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset + " DataSize=" + _dataSize; } }
    }
}