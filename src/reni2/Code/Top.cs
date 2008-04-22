using Reni.Context;

namespace Reni.Code
{
    internal abstract class Top : LeafElement
    {
        private readonly Size _destinationSize;
        private readonly Size _offset;
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _targetSize;

        protected Top(RefAlignParam refAlignParam, Size offset, Size targetSize, Size destinationSize)
        {
            _refAlignParam = refAlignParam;
            _offset = offset;
            _targetSize = targetSize;
            _destinationSize = destinationSize;
            StopByObjectId(945);
        }

        public override Size Size { get { return _destinationSize; } }
        public override Size DeltaSize { get { return Size*(-1); } }
        public override RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        public Size Offset { get { return _offset; } }
        public Size TargetSize { get { return _targetSize; } }
        public Size DestinationSize { get { return _destinationSize; } }
    }
}