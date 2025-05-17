using Reni.Basics;
using Reni.Context;

namespace Reni.Type;

sealed class Pair : TypeBase
{
    [EnableDump]
    readonly TypeBase First;

    [EnableDump]
    readonly TypeBase Second;

    [DisableDump]
    IEnumerable<string> InternalDeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null!;
        }
    }

    internal Pair(TypeBase first, TypeBase second)
    {
        First = first;
        Second = second;
        (First.Root == Second.Root).Assert();
    }

    internal override Root Root => First.Root;

    [DisableDump]
    internal override bool IsHollow => First.IsHollow && Second.IsHollow;

    protected override Size GetSize() => First.Size + Second.Size;

    [DisableDump]
    internal override string DumpPrintText
    {
        get
        {
            var result = "";
            var types = ToList;
            foreach(var t in types)
            {
                result += "\n";
                result += t;
            }

            return "(" + result.Indent() + "\n)";
        }
    }

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);


    [DisableDump]
    internal override TypeBase[] ToList
    {
        get
        {
            var result = new List<TypeBase>(First.ToList) { Second };
            return result.ToArray();
        }
    }

    protected override string GetNodeDump() => "pair." + ObjectId;
}