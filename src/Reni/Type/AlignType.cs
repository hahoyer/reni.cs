using Reni.Basics;
using Reni.Feature;
using Reni.Numeric;

namespace Reni.Type;

sealed class AlignType
    : Child<TypeBase>
{
    [DisableDump]
    readonly int AlignBits;

    [DisableDump]
    IEnumerable<string> InternalDeclarationOptions => Parent.DeclarationOptions;

    public AlignType(TypeBase target, int alignBits)
        : base(target)
    {
        AlignBits = alignBits;
        StopByObjectIds(-9);
        Parent.OverView.IsAligningPossible.Assert(Parent.Dump);
    }

    protected override string GetDumpPrintText()
        => "(" + Parent.OverView.DumpPrintText + ")" + AlignToken.TokenId + AlignBits;

    protected override bool GetIsHollow() => Parent.OverView.IsHollow;

    protected override IReference GetForcedReferenceForCache() => Parent.Make.ForcedReference;

    protected override Result GetDeAlign(Category category) => GetMutation(Parent) & category;
    protected override PointerType GetPointerForCache() => Parent.Make.ForcedPointer;
    internal override object GetDataValue(BitsConst data) => Parent.GetDataValue(data);

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    protected override IEnumerable<IConversion> GetSymmetricConversions() => base.GetSymmetricConversions().Concat
        ([Feature.Extension.Conversion(UnalignedResult)]);

    protected override bool GetIsAligningPossible() => false;

    protected override bool GetIsPointerPossible() => false;

    protected override Size GetSize() => Parent.OverView.Size.GetAlign(AlignBits);

    internal override int? GetSmartArrayLength(TypeBase elementType)
        => Parent.GetSmartArrayLength(elementType);

    internal override Result GetCopier(Category category) => Parent.GetCopier(category);

    internal override Result GetTypeOperatorApply(Result argResult)
        => Parent.GetTypeOperatorApply(argResult);

    protected override string GetNodeDump() => base.GetNodeDump() + "(" + Parent.NodeDump + ")";

    protected override Result ParentConversionResult(Category category)
        => UnalignedResult(category);

    public Result UnalignedResult(Category category)
        => Parent.GetResult(category, () => Make.ArgumentCode.GetBitCast(Parent.OverView.Size));
}