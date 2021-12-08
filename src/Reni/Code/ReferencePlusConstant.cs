using hw.DebugFormatter;
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
        internal readonly Size Right;

        public ReferencePlusConstant(Size right, string reason = null)
            : base(reason)
        {
            Right = right;
            AssertValid();
            StopByObjectIds();
        }

        void AssertValid()
        {
            Right.AssertAlignedSize(Root.DefaultRefAlignParam.AlignBits);
            (!Right.IsZero).Assert();
        }

        protected override string GetNodeDump() => base.GetNodeDump() + " Right=" + Right;

        internal override void Visit(IVisitor visitor) => visitor.ReferencePlus(Right);

        [DisableDump]
        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize;

        [DisableDump]
        internal override Size OutputSize => Root.DefaultRefAlignParam.RefSize;

        internal override CodeBase TryToCombineBack(TopRef precedingElement) => new TopRef(precedingElement.Offset + Right);

        internal override CodeBase TryToCombineBack(TopFrameRef precedingElement) => new TopFrameRef(precedingElement.Offset + Right);

        protected override FiberItem[] TryToCombineImplementation(FiberItem subsequentElement) => subsequentElement.TryToCombineBack(this);

        internal override FiberItem[] TryToCombineBack(ReferencePlusConstant precedingElement)
        {
            var newRight = Right + precedingElement.Right;
            if(newRight.IsZero)
                return new FiberItem[0];
            return new FiberItem[] {new ReferencePlusConstant(newRight)};
        }

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.ReferencePlusConstant(this);
    }
}