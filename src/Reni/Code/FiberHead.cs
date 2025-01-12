namespace Reni.Code
{
    abstract class FiberHead : CodeBase
    {
        static int NextObjectId;

        protected FiberHead(int objectId)
            : base(objectId)
        {
        }

        protected FiberHead()
            : base(NextObjectId++)
        {
        }

        [DisableDump]
        internal virtual bool IsNonFiberHeadList => false;

        protected virtual CodeBase TryToCombine(FiberItem subsequentElement) => null;

        internal override CodeBase Concat(FiberItem subsequentElement)
            => TryToCombine(subsequentElement) ?? new Fiber(this, subsequentElement);
    }
}