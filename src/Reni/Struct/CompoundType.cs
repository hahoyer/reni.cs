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
        => IsHollow? View.Find(tokenClass, true) : null;

    IImplementation? ISymbolProvider<DumpPrintToken>.Feature => IsHollow? Feature.Extension.Value(GetDumpPrintTokenResult) : null;

    IImplementation IMultiSymbolProviderForPointer<Definable>.GetFeature(Definable? tokenClass)
        => View.Find(tokenClass, true);

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult);

    [DisableDump]
    internal override Root Root => View.Root;

    [DisableDump]
    internal override CompoundView FindRecentCompoundView => View;

    [DisableDump]
    internal override bool IsHollow => View.IsHollow;

    internal override string DumpPrintText
    {
        get
        {
            if(IsDumpPrintTextActive)
                return "?";
            IsDumpPrintTextActive = true;
            var result = View.DumpPrintTextOfType;
            IsDumpPrintTextActive = false;
            return result;
        }
    }

    [DisableDump]
    internal override bool HasQuickSize => false;

    [DisableDump]
    internal override IEnumerable<IConversion> StripConversionsFromPointer
        => View.ConverterFeatures.Union(View.MixinConversions);

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    [DisableDump]
    protected override IEnumerable<IConversion> StripConversions
    {
        get
        {
            if(IsHollow)
                yield return Feature.Extension.Conversion(VoidConversion);
        }
    }

    [DisableDump]
    internal override ContextBase ToContext => View.Context;

    [DisableDump]
    internal override Issue[]? Issues => View.Issues;

    protected override Size GetSize() => View.CompoundViewSize;

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

    internal override Result GetCleanup(Category category) => View.Compound.Cleanup(category);

    Result VoidConversion(Category category) => GetMutation(Root.VoidType) & category;

    new Result GetDumpPrintTokenResult(Category category)
        => View.GetDumpPrintResultViaObject(category);
}