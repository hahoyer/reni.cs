using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Reference shift
    /// </summary>
    [Serializable]
    internal sealed class RefPlus : LeafElement
    {
        private readonly RefAlignParam _refAlignParam;

        [Node, DumpData(false)]
        private readonly Size _right;

        [Node, DumpData(false)]
        private readonly string _reason;

        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        public RefPlus(RefAlignParam refAlignParam, Size right, string reason)
        {
            _refAlignParam = refAlignParam;
            _right = right;
            _reason = reason;
            AssertValid();
            StopByObjectId(303);
        }

        private void AssertValid() { _right.AssertAlignedSize(RefAlignParam.AlignBits); }

        public override string NodeDump { get { return base.NodeDump + " Right=" + _right + " Reason=" + _reason; } }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        protected override Size GetInputSize() { return RefAlignParam.RefSize; }

        protected override string Format(StorageDescriptor start)
        {
            return start
                .CreateRefPlus(GetSize(), _right.SizeToPacketCount(RefAlignParam.AlignBits));
        }

        internal override LeafElement TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new TopRef(RefAlignParam, precedingElement.Offset + _right);
        }

        internal override LeafElement TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return new FrameRef(RefAlignParam, precedingElement.Offset + _right);
        }

        internal override LeafElement TryToCombine(LeafElement subsequentElement)
        {
            return subsequentElement
                .TryToCombineBack(this);
        }

        internal override LeafElement TryToCombineBack(RefPlus precedingElement)
        {
            return null;
            if (RefAlignParam.IsEqual(precedingElement.RefAlignParam))
                return new RefPlus(RefAlignParam, _right + precedingElement._right, _reason + " + " + precedingElement._reason);
            return base.TryToCombineBack(precedingElement);
        }
    }
}