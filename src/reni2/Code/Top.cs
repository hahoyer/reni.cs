using Reni.Context;

namespace Reni.Code
{
    internal abstract class Top : LeafElement
    {
        private readonly Size _offset;
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _size;

        protected Top(RefAlignParam refAlignParam, Size offset, Size size)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
            _size = size;
            StopByObjectId(-945);
        }

        public override Size Size { get { return _size; } }
        public override Size DeltaSize { get { return Size*(-1); } }
        public override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        public Size Offset { get { return _offset; } }
    }
}