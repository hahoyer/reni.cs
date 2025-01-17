using Reni.Feature;

namespace Reni.Type;

sealed class SymmetricClosureService
{
    internal interface INavigator
    {
        TypeBase Start(IConversion feature);
        TypeBase End(IConversion feature);
        IConversion? Combine(IConversion? startFeature, IConversion feature);
    }

    sealed class ForwardNavigator : INavigator
    {
        IConversion? INavigator.Combine(IConversion? start, IConversion end)
            => Feature.Combination.CheckedCreate(start, end);

        TypeBase INavigator.End(IConversion feature) => feature.ResultType();
        TypeBase INavigator.Start(IConversion feature) => feature.Source;
    }

    sealed class BackwardNavigator : INavigator
    {
        IConversion? INavigator.Combine(IConversion? start, IConversion end)
            => Feature.Combination.CheckedCreate(end, start);

        TypeBase INavigator.End(IConversion feature) => feature.Source;
        TypeBase INavigator.Start(IConversion feature) => feature.ResultType();
    }

    static readonly INavigator Forward = new ForwardNavigator();
    static readonly INavigator Backward = new BackwardNavigator();

    readonly INavigator Navigator;
    readonly TypeBase Source;
    readonly ValueCache<IEnumerable<IConversion>> ResultsCache;
    readonly ValueCache<IEnumerable<IConversion>> AllFeatures;
    internal IEnumerable<IConversion> Results => ResultsCache.Value;

    internal SymmetricClosureService(TypeBase source, INavigator? navigator = null)
    {
        Navigator = navigator ?? Forward;
        Source = source;
        AllFeatures = new(() => Source
            .SymmetricFeatureClosure()
            .ToArray());
        ResultsCache = new(Execute);
    }

    internal static IEnumerable<IConversion> To(TypeBase source)
        => new SymmetricClosureService(source, Backward).Results;

    IEnumerable<IConversion> Combination(List<TypeBase> foundTypes, IConversion? startFeature = null)
    {
        var startType = startFeature == null? Source : Navigator.End(startFeature);

        foreach(var feature in AllFeatures.Value.Where(feature => Navigator.Start(feature) == startType))
        {
            var destination = Navigator.End(feature);
            if(foundTypes.Contains(destination))
                continue;
            foundTypes.Add(destination);
            var newFeature = Navigator.Combine(startFeature, feature);
            if(newFeature == null)
                continue;
            yield return newFeature;
        }
    }

    IEnumerable<IConversion> Execute()
    {
        var results = new List<IConversion>();
        var foundTypes = new List<TypeBase>();
        var newFeatures = Combination(foundTypes).ToArray();

        while(newFeatures.Any())
        {
            results.AddRange(newFeatures);
            newFeatures = newFeatures.SelectMany(conversion => Combination(foundTypes, conversion)).ToArray();
        }

        return results.ToArray();
    }
}