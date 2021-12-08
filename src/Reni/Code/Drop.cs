using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code
{
    sealed class Drop : FiberItem
    {
        readonly Size BeforeSize;
        readonly Size AfterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            BeforeSize = beforeSize;
            AfterSize = afterSize;
            StopByObjectIds();
        }

        internal override void Visit(IVisitor visitor) => visitor.Drop(BeforeSize, AfterSize);

        [DisableDump]
        internal override Size InputSize => BeforeSize;

        [DisableDump]
        internal override Size OutputSize => AfterSize;

        protected override string GetNodeDump()
            => base.GetNodeDump() + " BeforeSize=" + BeforeSize + " AfterSize=" + AfterSize;

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.Drop(this);
    }
}