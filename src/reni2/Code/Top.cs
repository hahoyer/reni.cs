using System;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal abstract class Top : LeafElement
    {
        [Node]
        internal protected readonly Size Offset;
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _size;

        protected Top(RefAlignParam refAlignParam, Size offset, Size size)
        {
            _refAlignParam = refAlignParam;
            Offset = offset;
            _size = size;
            StopByObjectId(-945);
        }

        protected override Size GetSize()
        {
            return _size;
        }

        protected override Size GetDeltaSize()
        {
            return GetSize()*(-1);
        }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        public override string NodeDump { get { return base.NodeDump + " Offset="+Offset; } }
    }
}