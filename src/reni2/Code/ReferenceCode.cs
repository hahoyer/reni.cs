using Reni.Basics;
using Reni.Type;

namespace Reni.Code
{
    /// <summary>
    ///     ContextAtPosition reference, should be replaced
    /// </summary>
    sealed class ReferenceCode : FiberHead
    {
        static int _nextObjectId;

        [Node]
        internal readonly IContextReference Target;

        internal ReferenceCode(IContextReference target)
            : base(_nextObjectId++)
        {
            Target = target;
            StopByObjectIds();
        }


        protected override CodeArgs GetRefsImplementation() => CodeArgs.Create(Target);

        protected override Size GetSize() => Target.Size();

        protected override TCode VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.ContextRef(this);

        internal override void Visit(IVisitor visitor)
        {
            throw new UnexpectedContextReference(Target);
        }

        public override string DumpData() => Target.NodeDump();
    }
}