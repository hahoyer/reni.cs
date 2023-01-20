using Reni.Type;

namespace Reni.Code
{
    sealed class ArgumentTypeVisitor : Visitor<TypeBase, TypeBase>
    {
        internal override TypeBase Arg(Argument visitedObject) => visitedObject.Type;
        internal override TypeBase BitArray(BitArray visitedObject) => null;
        internal override TypeBase ContextRef(ReferenceCode visitedObject) => null;

        internal override TypeBase Fiber(Fiber visitedObject)
            => visitedObject.Visit(this);

        internal override TypeBase List(List visitedObject)
            => visitedObject.Visit(this);

        internal override TypeBase ThenElse(ThenElse visitedObject)
            => visitedObject.Visit(this);

        internal override TypeBase Default(CodeBase codeBase) => null;

        internal override TypeBase LocalReference(LocalReference visitedObject)
            => visitedObject.AlignedValueCode.Visit(this);
    }
}