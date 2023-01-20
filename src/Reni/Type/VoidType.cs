using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

sealed class VoidType : TypeBase, ISymbolProvider<DumpPrintToken>
{
    IImplementation ISymbolProvider<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
        => Feature.Extension.Value(DumpPrintTokenResult);

    [DisableDump]
    internal override Root Root => null;

    [DisableDump]
    internal override string DumpPrintText => "()";

    protected override string GetNodeDump() => "void";

    [DisableDump]
    internal override bool IsHollow => true;

    protected override TypeBase ReversePair(TypeBase first) => first;
    internal override TypeBase Pair(TypeBase second) => second;

    new Result DumpPrintTokenResult(Category category) => GetResult(category);
}