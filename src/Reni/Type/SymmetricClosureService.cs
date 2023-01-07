using Reni.Feature;

namespace Reni.Type;

sealed class SymmetricClosureService
{
    internal interface INavigator
    {
        TypeBase Start(IConversion feature);
        TypeBase End(IConversion feature);
        IConversion Combine(IConversion startFeature, IConversion feature);
    }

    sealed class ForwardNavigator : INavigator
    {
        IConversion INavigator.Combine(IConversion start, IConversion end)
            => Feature.Combination.CheckedCreate(start, end);

        TypeBase INavigator.End(IConversion feature) => feature.ResultType();
        TypeBase INavigator.Start(IConversion feature) => feature.Source;
    }

    sealed class BackwardNavigator : INavigator
    {
        IConversion INavigator.Combine(IConversion start, IConversion end)
            => Feature.Combination.CheckedCreate(end, start);

        TypeBase INavigator.End(IConversion feature) => feature.Source;
        TypeBase INavigator.Start(IConversion feature) => feature.ResultType();
    }

    internal static readonly INavigator Forward = new ForwardNavigator();
    static readonly INavigator Backward = new BackwardNavigator();

    TypeBase Source { get; }
    IEnumerable<IConversion> AllFeatures { get; }
    List<TypeBase> FoundTypes;
    List<IConversion> NewFeatures;

    internal SymmetricClosureService(TypeBase source)
    {
        Source = source;
        AllFeatures = source
            .SymmetricFeatureClosure()
            .ToArray();
    }

    internal static IEnumerable<IConversion> To(TypeBase source)
        => new SymmetricClosureService(source).Execute(Backward);

    IEnumerable<IConversion> Combination(INavigator navigator, IConversion startFeature = null)
    {
        var startType = Source;
        if(startFeature != null)
            startType = navigator.End(startFeature);

        foreach(var feature in AllFeatures.Where(feature => navigator.Start(feature) == startType))
        {
            var destination = navigator.End(feature);
            if(FoundTypes.Contains(destination))
                continue;
            FoundTypes.Add(destination);
            var newFeature = navigator.Combine(startFeature, feature);
            if(newFeature == null)
                continue;
            yield return newFeature;
            NewFeatures.Add(newFeature);
        }
    }

    internal IEnumerable<IConversion> Execute(INavigator navigator)
    {
        FoundTypes = new();
        NewFeatures = new();

        foreach(var feature in Combination(navigator))
            yield return feature;

        while(NewFeatures.Any())
        {
            var features = NewFeatures;
            NewFeatures = new();
            foreach(var feature in features.SelectMany(f => Combination(navigator, f)))
                yield return feature;
        }
    }
}