using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;

namespace Reni.Type
{
    sealed class SymmetricClosureService
    {
        internal static IEnumerable<ISimpleFeature> From(TypeBase source)
            => new SymmetricClosureService(source).Execute(_forward);
        internal static IEnumerable<ISimpleFeature> To(TypeBase source)
            => new SymmetricClosureService(source).Execute(_backward);

        interface INavigator
        {
            TypeBase Start(ISimpleFeature feature);
            TypeBase End(ISimpleFeature feature);
            ISimpleFeature Combine(ISimpleFeature startFeature, ISimpleFeature feature);
        }

        static readonly INavigator _forward = new ForwardNavigator();
        static readonly INavigator _backward = new BackwardNavigator();

        TypeBase Source { get; }
        IEnumerable<ISimpleFeature> AllFeatures { get; }
        List<TypeBase> _foundTypes;
        List<ISimpleFeature> _newFeatures;

        SymmetricClosureService(TypeBase source)
        {
            Source = source;
            AllFeatures = source
                .SymmetricFeatureClosure()
                .ToArray();
        }

        IEnumerable<ISimpleFeature> Combination(INavigator navigator, ISimpleFeature startFeature = null)
        {
            var startType = Source;
            if(startFeature != null)
                startType = navigator.End(startFeature);

            foreach(var feature in AllFeatures.Where(feature => navigator.Start(feature) == startType))
            {
                var destination = navigator.End(feature);
                if(_foundTypes.Contains(destination))
                    continue;
                var newFeature = navigator.Combine(startFeature, feature);
                yield return newFeature;
                _newFeatures.Add(newFeature);
                _foundTypes.Add(destination);
            }
        }

        IEnumerable<ISimpleFeature> Execute(INavigator navigator)
        {
            _foundTypes = new List<TypeBase>();
            _newFeatures = new List<ISimpleFeature>();

            foreach(var feature in Combination(navigator))
                yield return feature;

            while(_newFeatures.Any())
            {
                var features = _newFeatures;
                _newFeatures = new List<ISimpleFeature>();
                foreach(var feature in features.SelectMany(f => Combination(navigator, f)))
                    yield return feature;
            }
        }

        sealed class ForwardNavigator : INavigator
        {
            TypeBase INavigator.Start(ISimpleFeature feature) => feature.TargetType;
            TypeBase INavigator.End(ISimpleFeature feature) => feature.ResultType();

            ISimpleFeature INavigator.Combine(ISimpleFeature start, ISimpleFeature end)
                => start == null
                    ? end
                    : new Combination(start, end);
        }

        sealed class BackwardNavigator : INavigator
        {
            TypeBase INavigator.Start(ISimpleFeature feature) => feature.ResultType();
            TypeBase INavigator.End(ISimpleFeature feature) => feature.TargetType;

            ISimpleFeature INavigator.Combine(ISimpleFeature start, ISimpleFeature end)
                => start == null
                    ? end
                    : new Combination(end, start);
        }
    }
}