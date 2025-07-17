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
        Parent.IsAligningPossible.Assert(Parent.Dump);
    }

    [DisableDump]
    internal override string DumpPrintText
        => "(" + Parent.DumpPrintText + ")" + AlignToken.TokenId + AlignBits;

    [DisableDump]
    internal override bool IsHollow => Parent.IsHollow;

    protected override IReference GetForcedReferenceForCache() => Parent.ForcedReference;

    protected override Result GetDeAlign(Category category) => GetMutation(Parent) & category;
    protected override PointerType GetPointerForCache() => Parent.ForcedPointer;
    internal override object GetDataValue(BitsConst data) => Parent.GetDataValue(data);

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    [DisableDump]
    protected override IEnumerable<IConversion> RawSymmetricConversions
        =>
            base.RawSymmetricConversions.Concat
                ([Feature.Extension.Conversion(UnalignedResult)]);

    [DisableDump]
    internal override bool IsAligningPossible => false;

    [DisableDump]
    internal override bool IsPointerPossible => false;

    protected override Size GetSize() => Parent.Size.GetAlign(AlignBits);

    internal override int? GetSmartArrayLength(TypeBase elementType)
        => Parent.GetSmartArrayLength(elementType);

    internal override Result GetCopier(Category category) => Parent.GetCopier(category);

    internal override Result GetTypeOperatorApply(Result argResult)
        => Parent.GetTypeOperatorApply(argResult);

    protected override string GetNodeDump() => base.GetNodeDump() + "(" + Parent.NodeDump + ")";

    protected override Result ParentConversionResult(Category category)
        => UnalignedResult(category);

    public Result UnalignedResult(Category category)
        => Parent.GetResult(category, () => ArgumentCode.GetBitCast(Parent.Size));
}