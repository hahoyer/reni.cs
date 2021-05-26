using hw.DebugFormatter;

namespace Reni.Code
{
    abstract class FiberHead : CodeBase
    {
        static int _nextObjectId;

        protected FiberHead(int objectId)
            : base(objectId)
        {
        }

        protected FiberHead()
            : base(_nextObjectId++)
        {
        }

        [DisableDump]
        internal virtual bool IsNonFiberHeadList => false;

        protected virtual CodeBase TryToCombine(FiberItem subsequentElement) => null;

        internal override CodeBase Add(FiberItem subsequentElement)
            => TryToCombine(subsequentElement) ?? new Fiber(this, subsequentElement);
    }
}