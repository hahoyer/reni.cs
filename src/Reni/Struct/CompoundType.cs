using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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
        , ISymbolProviderForPointer<Definable>
        , ISymbolProvider<DumpPrintToken>
        , ISymbolProvider<Definable>
        , IChild<ContextBase>
{
    [Node]
    [DisableDump]
    internal CompoundView View { get; }

    bool IsDumpPrintTextActive;

    internal CompoundType(CompoundView view)
    {
        View = view;
        (!View.HasIssues).Assert(() => Tracer.Dump(View.Issues));
    }

    ContextBase IChild<ContextBase>.Parent => View.CompoundContext;

    IImplementation ISymbolProvider<Definable>.Feature(Definable tokenClass)
        => IsHollow? View.Find(tokenClass, true) : null;

    IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
        => IsHollow? Feature.Extension.Value(DumpPrintTokenResult) : null;

    IImplementation ISymbolProviderForPointer<Definable>.Feature(Definable tokenClass)
        => View.Find(tokenClass, true);

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        (DumpPrintToken tokenClass)
        => Feature.Extension.Value(DumpPrintTokenResult);

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

    internal override Issue[] Issues => View.Issues;

    protected override Size GetSize() => View.CompoundViewSize;

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + View.GetCompoundIdentificationDump() + ")";

    internal override Result Cleanup(Category category) => View.Compound.Cleanup(category);

    [DisableDump]
    IEnumerable<string> InternalDeclarationOptions => View.DeclarationOptions;

    Result VoidConversion(Category category) => Mutation(Root.VoidType) & category;

    new Result DumpPrintTokenResult(Category category)
        => View.DumpPrintResultViaObject(category);
}