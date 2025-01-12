using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    sealed class Assign : FiberItem
    {
        internal readonly Size TargetSize;

        public Assign(Size targetSize) { TargetSize = targetSize; }

        [DisableDump]
        internal override Size InputSize => Root.DefaultRefAlignParam.RefSize * 2;

        [DisableDump]
        internal override Size OutputSize => Size.Zero;

        protected override string GetNodeDump()
            => base.GetNodeDump() + " TargetSize=" + TargetSize;

        internal override void Visit(IVisitor visitor) => visitor.Assign(TargetSize);

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.Assign(this);
    }
}