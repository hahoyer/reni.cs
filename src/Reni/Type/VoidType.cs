using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

sealed class VoidType : TypeBase, ISymbolProvider<DumpPrintToken>
{
    IImplementation ISymbolProvider<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult);

    [DisableDump]
    internal override Root Root => null!;

    [DisableDump]
    internal override string DumpPrintText => "()";

    protected override string GetNodeDump() => "void";

    [DisableDump]
    internal override bool IsHollow => true;

    protected override TypeBase GetReversePair(TypeBase first) => first;
    internal override TypeBase GetPair(TypeBase second) => second;

    new Result GetDumpPrintTokenResult(Category category) => GetResult(category);
}