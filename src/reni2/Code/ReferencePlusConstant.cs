using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Reference shift
    /// </summary>
    sealed class ReferencePlusConstant : FiberItem
    {
        [DisableDump]
        readonly Size _right;

        public ReferencePlusConstant(Size right, string reason = null)
            : base(reason)
        {
            _right = right;
            AssertValid();
            StopByObjectIds(-4);
        }

        void AssertValid()
        {
            _right.AssertAlignedSize(Root.DefaultRefAlignParam.AlignBits);
            Tracer.Assert(!_right.IsZero);
        }

        protected override string GetNodeDump() => base.GetNodeDump() + " Right=" + _right;

        internal override void Visit(IVisitor visitor) => visitor.ReferencePlus(_right);

        [DisableDump]
        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize;

        [DisableDump]
        internal override Size OutputSize => Root.DefaultRefAlignParam.RefSize;

        internal override CodeBase TryToCombineBack(TopRef precedingElement) => new TopRef(precedingElement.Offset + _right);

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement) => new TopFrameRef(precedingElement.Offset + _right);

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) => subsequentElement.TryToCombineBack(this);

        internal override FiberItem[] TryToCombineBack(ReferencePlusConstant precedingElement)
        {
            var newRight = _right + precedingElement._right;
            if(newRight.IsZero)
                return new FiberItem[0];
            return new FiberItem[] {new ReferencePlusConstant(newRight)};
        }

    }
}