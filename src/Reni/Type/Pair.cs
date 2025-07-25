using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type;

sealed class Pair
    : TypeBase
        , IForcedConversionProvider<Pair>
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
        StopByObjectIds();
    }

    IEnumerable<IConversion> IForcedConversionProvider<Pair>.GetResult(Pair destination)
    {
        var sourceList = ToList;
        var destinationList = destination.ToList;
        if(sourceList.Length != destinationList.Length)
            return [];

        var results = sourceList
            .Select((element, index) => ConversionService.FindPath(element, destinationList[index]))
            .ToArray();

        NotImplementedMethod(destination);
        return [];
    }

    [DisableDump]
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
    protected override IEnumerable<IGenericProviderForType> GenericList
        => this.GenericListFromType(base.GenericList);

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
