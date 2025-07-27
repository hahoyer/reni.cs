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

    [DisableDump]
    internal override bool IsHollow => false;

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize + IndexType.Size;

    protected override CodeBase GetSetterCode()
        => GetPair(ValueType.IsHollow? ValueType : ValueType.Make.Pointer)
            .Make.ArgumentCode
            .GetArraySetter(ValueType.Size, IndexType.Size);

    protected override CodeBase GetGetterCode()
        => Make.ArgumentCode.GetArrayGetter(ValueType.Size, IndexType.Size);

    internal Result GetResult(Category category, Result leftResult, TypeBase right)
    {
        var rightResult = right
            .GetConversion(category | Category.Type, IndexType)
            .AutomaticDereferencedAlignedResult;

        return GetResult(category, (leftResult + rightResult)!);
    }
}