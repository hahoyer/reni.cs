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

    protected override bool GetIsHollow() => First.OverView.IsHollow && Second.OverView.IsHollow;

    protected override Size GetSize() => First.OverView.Size + Second.OverView.Size;

    protected override string GetDumpPrintText()
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

    TypeBase[] ToList => GetToList().ToArray();

    [DisableDump]
    internal override IEnumerable<string> DeclarationOptions
        => base.DeclarationOptions.Concat(InternalDeclarationOptions);

    protected override IEnumerable<IGenericProviderForType> GetGenericProviders() => this.GetGenericProviders(base.GetGenericProviders());

    internal override IEnumerable<TypeBase> GetToList() 
        => First.GetToList().Concat(Second.GetToList());

    protected override string GetNodeDump() => "pair." + ObjectId;
}
