using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;

namespace Reni.Type
{
    sealed class SymmetricClosureService
    {
        internal static IEnumerable<IConversion> To(TypeBase source)
            => new SymmetricClosureService(source).Execute(Backward);

        internal interface INavigator
        {
            TypeBase Start(IConversion feature);
            TypeBase End(IConversion feature);
            IConversion Combine(IConversion startFeature, IConversion feature);
        }

        internal static readonly INavigator Forward = new ForwardNavigator();
        static readonly INavigator Backward = new BackwardNavigator();

        TypeBase Source { get; }
        IEnumerable<IConversion> AllFeatures { get; }
        List<TypeBase> _foundTypes;
        List<IConversion> _newFeatures;

        internal SymmetricClosureService(TypeBase source)
        {
            Source = source;
            AllFeatures = source
                .SymmetricFeatureClosure()
                .ToArray();
        }

        IEnumerable<IConversion> Combination(INavigator navigator, IConversion startFeature = null)
        {
            var startType = Source;
            if(startFeature != null)
                startType = navigator.End(startFeature);

            foreach(var feature in AllFeatures.Where(feature => navigator.Start(feature) == startType))
            {
                var destination = navigator.End(feature);
                if(_foundTypes.Contains(destination))
                    continue;
                _foundTypes.Add(destination);
                var newFeature = navigator.Combine(startFeature, feature);
                if(newFeature == null)
                    continue;
                yield return newFeature;
                _newFeatures.Add(newFeature);
            }
        }

        internal IEnumerable<IConversion> Execute(INavigator navigator)
        {
            _foundTypes = new List<TypeBase>();
            _newFeatures = new List<IConversion>();

            foreach(var feature in Combination(navigator))
                yield return feature;

            while(_newFeatures.Any())
            {
                var features = _newFeatures;
                _newFeatures = new List<IConversion>();
                foreach(var feature in features.SelectMany(f => Combination(navigator, f)))
                    yield return feature;
            }
        }

        sealed class ForwardNavigator : INavigator
        {
            TypeBase INavigator.Start(IConversion feature) => feature.Source;
            TypeBase INavigator.End(IConversion feature) => feature.ResultType();

            IConversion INavigator.Combine(IConversion start, IConversion end)
                => Feature.Combination.CheckedCreate(start, end);
        }

        sealed class BackwardNavigator : INavigator
        {
            TypeBase INavigator.Start(IConversion feature) => feature.ResultType();
            TypeBase INavigator.End(IConversion feature) => feature.Source;

            IConversion INavigator.Combine(IConversion start, IConversion end)
                => Feature.Combination.CheckedCreate(end, start);
        }
    }
}