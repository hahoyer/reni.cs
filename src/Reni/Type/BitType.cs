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
            return null!;
        }
    }

    internal BitType(Root root) => Root = root;

    IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature
        => Feature.Extension.Value(GetDumpPrintTokenResult, this);

    protected override string GetDumpPrintText() => "bit";

    protected override bool GetIsHollow() => false;

    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    protected override string GetNodeDump() => "bit";
    protected override Size GetSize() => Size.Bit;

    protected override string Dump(bool isRecursion) => GetType().PrettyName();

    [DisableDump]
    protected override CodeBase DumpPrintCode => Make.Align.Make.ArgumentCode.GetDumpPrintNumber();

    internal NumberType Number(int bitCount) => GetArray(bitCount).Number;

    internal Result GetResult(Category category, BitsConst bitsConst) => Number(bitsConst.Size.ToInt())
        .GetResult(category, () => Code.Extension.GetCode(bitsConst));
}
