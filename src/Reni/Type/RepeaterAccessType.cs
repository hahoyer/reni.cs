using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type;

sealed class RepeaterAccessType
    : DataSetterTargetType
{
    [DisableDump]
    protected override bool IsMutable { get; }

    [DisableDump]
    internal override TypeBase ValueType { get; }

    [DisableDump]
    TypeBase IndexType { get; }

    internal RepeaterAccessType(IRepeaterType repeaterType)
        : base(repeaterType.Root)
    {
        IsMutable = repeaterType.IsMutable;
        ValueType = repeaterType.ElementType;
        IndexType = repeaterType.IndexType;
    }

    protected override bool GetIsHollow() => false;

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize + IndexType.OverView.Size;

    protected override CodeBase GetSetterCode()
        => GetPair(ValueType.OverView.IsHollow ? ValueType : ValueType.Make.Pointer)
            .Make.ArgumentCode
            .GetArraySetter(ValueType.OverView.Size, IndexType.OverView.Size);

    protected override CodeBase GetGetterCode()
        => Make.ArgumentCode.GetArrayGetter(ValueType.OverView.Size, IndexType.OverView.Size);

    internal Result GetResult(Category category, Result leftResult, TypeBase right)
    {
        var rightResult = right
            .GetConversion(category | Category.Type, IndexType)
            .AutomaticDereferencedAligned;

        return GetResult(category, (leftResult + rightResult)!);
    }
}