using hw.Scanner;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Struct;

sealed class CompoundContext
    : Child
        , IMultiSymbolProviderForPointer<Definable>
        , IContextReference
{
    [DisableDump]
    internal CompoundView View { get; }

    readonly int Order;

    internal CompoundContext(CompoundView view)
        : base(view.Compound.Parent)
    {
        Order = Closures.NextOrder++;
        View = view;
        StopByObjectIds();
    }

    int IContextReference.Order => Order;

    IImplementation? IMultiSymbolProviderForPointer<Definable>.GetFeature(Definable tokenClass)
        => View.Find(tokenClass, false);

    protected override string GetContextIdentificationDumpAsChild()
        => View.GetContextDump();

    protected override SourcePosition GetMainPosition() => View.Compound.Syntax.MainAnchor.FullToken.Start;

    internal override CompoundView GetRecentCompoundView() => View;

    internal override string GetPositionInformation(SourcePosition target)
        => View.Compound.Syntax.GetChildPositionDump(target) ?? "";

    [DisableDump]
    protected override string LevelFormat => "compount";

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions => View.DeclarationOptions;

    internal string? GetChildPositionDump(Syntax target)
        => View.Compound.Syntax.GetChildPositionDump(target.MainAnchor.Token.Start);
}
