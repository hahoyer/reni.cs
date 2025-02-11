using Reni.Basics;
using Reni.Feature;

namespace Reni.Type;

sealed class StableReferenceType
    : TagChild<PointerType>
{
    internal StableReferenceType(PointerType parent)
        : base(parent) { }

    [DisableDump]
    internal override bool IsAligningPossible => false;

    [DisableDump]
    internal override bool IsPointerPossible => false;

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
        => base.StripConversions
            .Concat([Feature.Extension.Conversion(ConvertToPointer)]);

    [DisableDump]
    internal override TypeBase Weaken => Parent;

    protected override string TagTitle => "stable";

    Result ConvertToPointer(Category category) => GetMutation(Parent) & category;
}