using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Reference shift
    /// </summary>
    [Serializable]
    internal sealed class RefPlus : FiberItem
    {
        private readonly RefAlignParam _refAlignParam;

        [Node, DisableDump]
        private readonly Size _right;

        [Node, DisableDump]
        private readonly string _reason;

        [DisableDump]
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        public RefPlus(RefAlignParam refAlignParam, Size right, string reason)
        {
            _refAlignParam = refAlignParam;
            _right = right;
            _reason = reason;
            AssertValid();
            StopByObjectId(-10);
        }

        private void AssertValid()
        {
            _right.AssertAlignedSize(RefAlignParam.AlignBits);
            Tracer.Assert(!_right.IsZero);
        }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " Right=" + _right + " Reason=" + _reason; } }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.RefPlus(GetSize(), _right); }

        [DisableDump]
        internal override Size InputSize { get { return GetSize(); } }

        [DisableDump]
        internal override Size OutputSize { get { return GetSize(); } }

        private Size GetSize() { return RefAlignParam.RefSize; }

        internal override CodeBase TryToCombineBack(TopRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            var reason = _reason + "(" + _right + ") <= " + precedingElement.Reason;
            return new TopRef(RefAlignParam, precedingElement.Offset + _right, reason);
        }

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            var reason = _reason + "(" + _right + ") <= " + precedingElement.Reason;
            return new TopFrameRef(RefAlignParam, precedingElement.Offset + _right, reason);
        }

        internal override FiberItem[] TryToCombine(FiberItem subsequentElement) { return subsequentElement.TryToCombineBack(this); }

        internal override FiberItem[] TryToCombineBack(RefPlus precedingElement)
        {
            if(RefAlignParam.IsEqual(precedingElement.RefAlignParam))
            {
                var reason = _reason + "(" + _right + ") <= " + precedingElement._reason;
                var newRight = _right + precedingElement._right;
                if(newRight.IsZero)
                    return new FiberItem[0];
                return new[] {new RefPlus(RefAlignParam, newRight, reason)};
            }
            return base.TryToCombineBack(precedingElement);
        }

        internal override CodeBase TryToCombineBack(LocalVariableReference precedingElement)
        {
            Tracer.Assert(RefAlignParam.Equals(precedingElement.RefAlignParam));
            return CodeBase
                .LocalVariableReference(RefAlignParam, precedingElement.Holder, precedingElement.Offset + _right);
        }
    }
}