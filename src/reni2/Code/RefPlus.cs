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
    internal sealed class RefPlus : FiberItem
    {
        private readonly RefAlignParam _refAlignParam;

        [Node, IsDumpEnabled(false)]
        private readonly Size _right;

        [Node, IsDumpEnabled(false)]
        private readonly string _reason;

        [IsDumpEnabled(false)]
        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        public RefPlus(RefAlignParam refAlignParam, Size right, string reason)
        {
            _refAlignParam = refAlignParam;
            _right = right;
            _reason = reason;
            AssertValid();
            StopByObjectId(-124);
        }

        private void AssertValid() { _right.AssertAlignedSize(RefAlignParam.AlignBits); }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " Right=" + _right + " Reason=" + _reason; } }
        internal override void Execute(IFormalMaschine formalMaschine) { formalMaschine.RefPlus(GetSize(), _right); }

        internal override Size InputSize { get { return GetSize(); } }
        internal override Size OutputSize { get { return GetSize(); } }

        private Size GetSize() { return RefAlignParam.RefSize; }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            var reason = _reason + "(" + _right + ") + " + precedingElement.Reason;
            return new TopRef(RefAlignParam, precedingElement.Offset + _right, reason);
        }

        internal override CodeBase TryToCombineBack(FrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            var reason = _reason + "(" + _right + ") + " + precedingElement.Reason;
            return new FrameRef(RefAlignParam, precedingElement.Offset + _right, reason);
        }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement)
        {
            return subsequentElement.TryToCombineBack(this);
        }

        internal override FiberItem[] TryToCombineBack(RefPlus precedingElement)
        {
            if (RefAlignParam.IsEqual(precedingElement.RefAlignParam))
            {
                var reason = _reason + "(" + _right + ") + " + precedingElement._reason;
                return new[]{new RefPlus(RefAlignParam, _right + precedingElement._right, reason)};
            }
            return base.TryToCombineBack(precedingElement);
        }
    }
}