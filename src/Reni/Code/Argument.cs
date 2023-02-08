using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    sealed class Argument : FiberHead
    {
        static int NextObjectId;

        [Node]
        internal TypeBase Type { get; }

        internal Argument(TypeBase type)
            : base(NextObjectId++)
        {
            Type = type;
            StopByObjectIds();
        }

        protected override Size GetSize() => Type.Size;
        protected override Closures GetRefsImplementation() => Closures.GetArgument();
        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual) => actual.Argument(this);
    }
}