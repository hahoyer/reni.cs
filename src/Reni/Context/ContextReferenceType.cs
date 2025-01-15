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

    [DisableDump]
    internal override CompoundView FindRecentCompoundView => Parent;

    [DisableDump]
    internal override bool IsHollow => Parent.IsHollow;

    [DisableDump]
    internal override bool IsPointerPossible => !IsHollow;

    protected override Size GetSize() => IsHollow? Size.Zero : Root.DefaultRefAlignParam.RefSize;

    [DisableDump]
    protected override CodeBase DumpPrintCode => ContextOperator.TokenId.GetDumpPrintTextCode();

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);


    protected override IEnumerable<IConversion> StripConversions
        => base.StripConversions
            .Concat([Feature.Extension.Conversion(PointerConversion)]);

    new Result GetDumpPrintTokenResult(Category category)
        => Root.VoidType
            .GetResult(category, () => DumpPrintCode);

    Result PointerConversion(Category category)
        => Parent
            .Type
            .Pointer
            .GetResult
            (
                category
                , c => GetArgumentResult(c)
                        .AddToReference(() => Parent.CompoundViewSize! * -1)
                    ?? throw new InvalidOperationException()
            );
}