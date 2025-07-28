using Reni.Basics;
using Reni.Feature;

namespace Reni.Type;

sealed class StableReferenceType
    : TagChild<PointerType>
{
    internal StableReferenceType(PointerType parent)
        : base(parent) { }

    protected override bool GetIsAligningPossible() => false;

    protected override bool GetIsPointerPossible() => false;

    protected override IEnumerable<IConversion> GetStripConversions() => base.GetStripConversions()
        .Concat([Feature.Extension.Conversion(ConvertToPointer)]);

    [DisableDump]
    internal override TypeBase Weaken => Parent;

    protected override string TagTitle => "stable";

    Result ConvertToPointer(Category category) => GetMutation(Parent) & category;
}