using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

sealed class VoidType : TypeBase, ISymbolProvider<DumpPrintToken>
{
    [DisableDump]
    internal override Root Root { get; }

    internal VoidType(Root root) => Root = root;
    
    IImplementation ISymbolProvider<DumpPrintToken>.Feature => Feature.Extension.Value(GetDumpPrintTokenResult);

    protected override string GetDumpPrintText() => "()";

    protected override string GetNodeDump() => "void";

    protected override bool GetIsHollow() => true;

    protected override TypeBase GetReversePair(TypeBase first) => first;
    internal override TypeBase GetPair(TypeBase second) => second;

    new Result GetDumpPrintTokenResult(Category category) => GetResult(category);
}
