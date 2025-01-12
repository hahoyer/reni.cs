#nullable enable
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Struct;

sealed class CompoundContext : Child, IMultiSymbolProviderForPointer<Definable>, IContextReference
{
    [DisableDump]
    internal CompoundView View { get; }

    readonly int Order;

    [EnableDump]
    ValueSyntax[] Syntax => View.Compound.Syntax.PureStatements;

    internal CompoundContext(CompoundView view)
        : base(view.Compound.Parent)
    {
        Order = Closures.NextOrder++;
        View = view;
        StopByObjectIds();
    }

    int IContextReference.Order => Order;

    IImplementation IMultiSymbolProviderForPointer<Definable>.GetFeature(Definable? tokenClass)
        => View.Find(tokenClass, false);

    protected override string ContextChildIdentificationDump => GetCompoundIdentificationDump();

    internal override CompoundView GetRecentCompoundView() => View;

    [DisableDump]
    protected override string LevelFormat => "compount";

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions => View.DeclarationOptions;

    string GetCompoundIdentificationDump() => View.GetCompoundChildDump();
}