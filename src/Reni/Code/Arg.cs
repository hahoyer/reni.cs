using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    sealed class Arg : FiberHead
    {
        static int NextObjectId;

        [Node]
        internal TypeBase Type { get; }

        internal Arg(TypeBase type)
            : base(NextObjectId++)
        {
            Type = type;
            StopByObjectIds();
        }

        protected override Size GetSize() => Type.Size;
        protected override Closures GetRefsImplementation() => Closures.Arg();
        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual) => actual.Arg(this);
    }
}