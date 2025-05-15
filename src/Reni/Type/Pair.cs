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
        Root = GetRoot();
    }

    [DisableDump]
    internal override bool IsHollow => First.IsHollow && Second.IsHollow;

    [DisableDump]
    internal override Root Root { get; }

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

    Root GetRoot()
    {
        var first = First.Root;
        var second = Second.Root;

        (first == null || second == null || first == second).Assert();
        var result = first ?? second;
        result.AssertIsNotNull();
        return result!;
    }
}