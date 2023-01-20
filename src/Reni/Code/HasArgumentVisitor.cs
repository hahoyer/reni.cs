namespace Reni.Code;

sealed class HasArgumentVisitor : Visitor<bool, bool>
{
    internal override bool Arg(Argument visitedObject) => true;
    internal override bool BitArray(BitArray visitedObject) => false;
    internal override bool ContextRef(ReferenceCode visitedObject) => false;
    internal override bool Fiber(Fiber visitedObject) => visitedObject.HasArg;

    internal override bool List(List visitedObject)
        => visitedObject.Data.Any(x => x.Visit(this));

    internal override bool Default(CodeBase codeBase) => false;

    internal override bool LocalReference(LocalReference visitedObject)
        => visitedObject.AlignedValueCode.Visit(this);
}