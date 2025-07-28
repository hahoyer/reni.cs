using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

sealed class CompoundType
    : TypeBase
        , ISymbolProviderForPointer<DumpPrintToken>
        , IMultiSymbolProviderForPointer<Definable>
        , ISymbolProvider<DumpPrintToken>
        , IMultiSymbolProvider<Definable>
        , IChild<ContextBase>
{
    [Node]
    [DisableDump]
    internal CompoundView View { get; }

    bool IsDumpPrintTextActive;

    [DisableDump]
    IEnumerable<string> InternalDeclarationOptions => View.DeclarationOptions;

    internal CompoundType(CompoundView view) => View = view;

    ContextBase IChild<ContextBase>.Parent => View.CompoundContext;

    IImplementation? IMultiSymbolProvider<Definable>.GetFeature(Definable tokenClass)
        => GetIsHollow()? View.Find(tokenClass, true) : null;

    IImplementation? IMultiSymbolProviderForPointer<Definable>.GetFeature(Definable tokenClass)
        => View.Find(tokenClass, true);

    IImplementation? ISymbolProvider<DumpPrintToken>.Feature
        => GetIsHollow()? Feature.Extension.Value(GetDumpPrintTokenResult) : null;

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        => Feature.Extension.Value(GetDumpPrintTokenResult);

    [DisableDump]
    internal override Root Root => View.Root;

    [DisableDump]
    internal int Count => View.Compound.Syntax.EndPosition;

    internal override CompoundView FindRecentCompoundView() => View;

    protected override bool GetIsHollow() => View.IsHollow;

    protected override string GetDumpPrintText()
    {
        if(IsDumpPrintTextActive)
            return "?";
        IsDumpPrintTextActive = true;
        var result = View.DumpPrintTextOfType;
        IsDumpPrintTextActive = false;
        return result;
    }

    protected override bool GetHasQuickSize() => false;

    protected override IEnumerable<IConversion> GetStripConversionsFromPointer()
        => View.ConverterFeatures.Union(View.MixinConversions).Union(View.KernelPartConversions);

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    protected override IEnumerable<IConversion> GetStripConversions()
    {
        if(GetIsHollow())
            yield return Feature.Extension.Conversion(VoidConversion);
    }

    [DisableDump]
    internal override ContextBase ToContext => View.Context;

    [DisableDump]
    internal override Issue[] Issues => View.Issues;

    protected override Size GetSize() => View.CompoundViewSize!;

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

    internal override Result GetCleanup(Category category) => View.Compound.Cleanup(category);

    Result VoidConversion(Category category) => GetMutation(Root.VoidType) & category;

    new Result GetDumpPrintTokenResult(Category category)
        => View.GetDumpPrintResultViaObject(category);
}