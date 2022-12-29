using hw.DebugFormatter;
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
    {
        IsMutable = repeaterType.IsMutable;
        ValueType = repeaterType.ElementType;
        IndexType = repeaterType.IndexType;
    }

    [DisableDump]
    internal override bool IsHollow => false;

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize + IndexType.Size;

    protected override CodeBase SetterCode()
        => Pair(ValueType.SmartPointer)
            .ArgCode
            .ArraySetter(ValueType.Size, IndexType.Size);

    protected override CodeBase GetterCode()
        => ArgCode.ArrayGetter(ValueType.Size, IndexType.Size);

    internal Result Result(Category category, Result leftResult, TypeBase right)
    {
        var rightResult = right
            .Conversion(category | Category.Type, IndexType)
            .AutomaticDereferencedAlignedResult();

        return Result(category, leftResult + rightResult);
    }
}