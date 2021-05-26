using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    sealed class Arg : FiberHead
    {
        static int _nextObjectId;

        [Node]
        internal TypeBase Type { get; }

        internal Arg(TypeBase type)
            : base(_nextObjectId++)
        {
            Type = type;
            StopByObjectIds();
        }

        protected override Size GetSize() => Type.Size;
        protected override Closures GetRefsImplementation() => Closures.Arg();
        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual) => actual.Arg(this);
    }
}