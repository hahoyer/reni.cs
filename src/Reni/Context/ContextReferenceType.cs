using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Struct;
using Reni.Type;

namespace Reni.Context;

sealed class ContextReferenceType
    : TypeBase
        , ISymbolProviderForPointer<DumpPrintToken>
{
    readonly CompoundView Parent;

    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null!;
        }
    }

    internal ContextReferenceType(CompoundView parent) => Parent = parent;

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult, this);

    [DisableDump]
    internal override Root Root => Parent.Root;

    internal override CompoundView FindRecentCompoundView() => Parent;

    protected override bool GetIsHollow() => Parent.IsHollow;

    protected override bool GetIsPointerPossible() => !GetIsHollow();

    protected override Size GetSize() => GetIsHollow()? Size.Zero : Root.DefaultRefAlignParam.RefSize;

    [DisableDump]
    protected override CodeBase DumpPrintCode => ContextOperator.TokenId.GetDumpPrintTextCode();

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);


    protected override IEnumerable<IConversion> GetStripConversions() => base.GetStripConversions()
        .Concat([Feature.Extension.Conversion(PointerConversion)]);

    new Result GetDumpPrintTokenResult(Category category)
        => Root.VoidType
            .GetResult(category, () => DumpPrintCode);

    Result PointerConversion(Category category)
        => Parent
            .Type.Make.Pointer
            .GetResult
            (
                category
                , c => GetArgumentResult(c)
                        .AddToReference(() => Parent.CompoundViewSize! * -1)
                    ?? throw new InvalidOperationException()
            );
}