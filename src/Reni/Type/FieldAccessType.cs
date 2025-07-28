using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;

namespace Reni.Type;

sealed class FieldAccessType : DataSetterTargetType
{
    [DisableDump]
    CompoundView View { get; }

    [DisableDump]
    int Position { get; }

    [DisableDump]
    Size FieldOffset => View.FieldOffset(Position);

    internal FieldAccessType(CompoundView compoundView, int position)
        : base(compoundView.Root)
    {
        View = compoundView;
        Position = position;
    }

    protected override bool IsMutable => View.Compound.Syntax.IsMutable(Position);

    [DisableDump]
    internal override TypeBase ValueType => View.ValueType(Position);

    protected override bool GetIsHollow() => false;

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

    protected override CodeBase GetGetterCode()
        => Make.ArgumentCode.GetReferenceWithOffset(FieldOffset);

    protected override CodeBase GetSetterCode()
        => GetPair(ValueType.Make.ForcedPointer)
            .Make.ArgumentCode
            .GetAssignment(ValueType.OverView.Size);

    internal override Result DestinationResult(Category category) => base
        .DestinationResult(category)
        .AddToReference(() => FieldOffset);

    internal override int? GetSmartArrayLength(TypeBase elementType)
        => ValueType.GetSmartArrayLength(elementType);

    [DisableDump]
    internal override ContextBase ToContext => ValueType.ToContext;

    string GetCompoundIdentificationDump()
        => View.GetCompoundIdentificationDump() + ":" + Position;
}