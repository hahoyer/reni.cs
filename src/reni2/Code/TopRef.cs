using System;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal sealed class TopRef : RefLeafElement
    {
        public TopRef(RefAlignParam refAlignParam, Size offset) : base(refAlignParam,offset)
        {
            StopByObjectId(7320);
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.TopRef(RefAlignParam, Offset);
        }

        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }
    }

    [Serializable]
    internal sealed class FrameRef : RefLeafElement
    {
        public FrameRef(RefAlignParam refAlignParam, Size offset)
            : base(refAlignParam, offset)
        {
            StopByObjectId(547);
        }

        protected override string Format(StorageDescriptor start)
        {
            return start.FrameRef(RefAlignParam, Offset);
        }

        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }
    }
}