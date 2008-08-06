using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Dereferencing operation
    /// </summary>
    [Serializable]
    internal sealed class Dereference : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly Size _size;

        public Dereference(RefAlignParam refAlignParam, Size size)
        {
            _refAlignParam = refAlignParam;
            _size = size;
            StopByObjectId(-399);
        }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        protected override Size GetSize()
        {
            return _size;
        }

        protected override Size GetDeltaSize()
        {
            return RefAlignParam.RefSize - GetSize();
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.Unref(RefAlignParam, _size);
        }

        internal override LeafElement[] TryToCombineN(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBackN(this);
        }

        internal override LeafElement TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopData(RefAlignParam, precedingElement.Offset, GetSize());
        }

        internal override LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopFrame(RefAlignParam, precedingElement.Offset, GetSize());
        }
    }
}