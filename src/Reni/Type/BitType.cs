using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

sealed class BitType : TypeBase, ISymbolProviderForPointer<DumpPrintToken>
{
    [DisableDump]
    internal override Root Root { get; }

    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null;
        }
    }

    internal BitType(Root root) => Root = root;

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        (DumpPrintToken tokenClass)
        => Feature.Extension.Value(GetDumpPrintTokenResult, this);

    [DisableDump]
    internal override string DumpPrintText => "bit";

    [DisableDump]
    internal override bool IsHollow => false;

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    protected override string GetNodeDump() => "bit";
    protected override Size GetSize() => Size.Bit;

    protected override string Dump(bool isRecursion) => GetType().PrettyName();

    [DisableDump]
    protected override CodeBase DumpPrintCode => Align.ArgumentCode.DumpPrintNumber();

    internal NumberType Number(int bitCount) => GetArray(bitCount).Number;

    internal Result GetResult(Category category, BitsConst bitsConst) => Number(bitsConst.Size.ToInt())
        .GetResult(category, () => CodeBase.BitsConst(bitsConst));
}