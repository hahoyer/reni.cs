using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal abstract class Top : LeafElement
    {
        [Node, DumpData(false)]
        internal protected readonly Size Offset;
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

        protected override Size GetSize()
        {
            return _size;
        }

        protected override Size GetInputSize()
        {
            return Size.Zero;
        }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        protected Size DataSize { get { return _dataSize; } }

        public override string NodeDump { get { return base.NodeDump + " Offset=" + Offset + " DataSize=" + _dataSize; } }
    }
}