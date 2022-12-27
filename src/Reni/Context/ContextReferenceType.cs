using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null;
        }
    }

    internal ContextReferenceType(CompoundView parent) => FindRecentCompoundView = parent;

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
        => Feature.Extension.Value(DumpPrintTokenResult, this);

    [DisableDump]
    internal override Root Root => FindRecentCompoundView.Root;

    [DisableDump]
    internal override CompoundView FindRecentCompoundView { get; }

    [DisableDump]
    internal override bool IsHollow => false;

    [DisableDump]
    internal override bool IsPointerPossible => true;

    protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

    protected override CodeBase DumpPrintCode()
        => CodeBase.DumpPrintText(ContextOperator.TokenId);

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);


    protected override IEnumerable<IConversion> StripConversions
        => base.StripConversions
            .Concat(new[] { Feature.Extension.Conversion(PointerConversion) });

    new Result DumpPrintTokenResult(Category category)
        => VoidType
            .Result(category, DumpPrintCode);

    Result PointerConversion(Category category)
        => FindRecentCompoundView
            .Type
            .Pointer
            .Result
                (category, c => ArgResult(c).AddToReference(() => FindRecentCompoundView.CompoundViewSize * -1));
}