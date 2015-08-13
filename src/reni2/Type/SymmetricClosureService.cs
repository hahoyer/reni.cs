using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;

namespace Reni.Type
{
    sealed class SymmetricClosureService
    {
        internal static IEnumerable<IValueFeature> From(TypeBase source)
            => new SymmetricClosureService(source).Execute(_forward);
        internal static IEnumerable<IValueFeature> To(TypeBase source)
            => new SymmetricClosureService(source).Execute(_backward);

        interface INavigator
        {
            TypeBase Start(IValueFeature feature);
            TypeBase End(IValueFeature feature);
            IValueFeature Combine(IValueFeature startFeature, IValueFeature feature);
        }

        static readonly INavigator _forward = new ForwardNavigator();
        static readonly INavigator _backward = new BackwardNavigator();

        TypeBase Source { get; }
        IEnumerable<IValueFeature> AllFeatures { get; }
        List<TypeBase> _foundTypes;
        List<IValueFeature> _newFeatures;

        SymmetricClosureService(TypeBase source)
        {
            Source = source;
            AllFeatures = source
                .SymmetricFeatureClosure()
                .ToArray();
        }

        IEnumerable<IValueFeature> Combination(INavigator navigator, IValueFeature startFeature = null)
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

        IEnumerable<IValueFeature> Execute(INavigator navigator)
        {
            _foundTypes = new List<TypeBase>();
            _newFeatures = new List<IValueFeature>();

            foreach(var feature in Combination(navigator))
                yield return feature;

            while(_newFeatures.Any())
            {
                var features = _newFeatures;
                _newFeatures = new List<IValueFeature>();
                foreach(var feature in features.SelectMany(f => Combination(navigator, f)))
                    yield return feature;
            }
        }

        sealed class ForwardNavigator : INavigator
        {
            TypeBase INavigator.Start(IValueFeature feature) => feature.TargetType;
            TypeBase INavigator.End(IValueFeature feature) => feature.ResultType();

            IValueFeature INavigator.Combine(IValueFeature start, IValueFeature end)
                => Feature.Combination.CheckedCreate(start, end);
        }

        sealed class BackwardNavigator : INavigator
        {
            TypeBase INavigator.Start(IValueFeature feature) => feature.ResultType();
            TypeBase INavigator.End(IValueFeature feature) => feature.TargetType;

            IValueFeature INavigator.Combine(IValueFeature start, IValueFeature end)
                => Feature.Combination.CheckedCreate(end, start);
        }
    }
}