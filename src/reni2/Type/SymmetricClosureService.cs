using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;

namespace Reni.Type
{
    sealed class SymmetricClosureService
    {
        internal static IEnumerable<IValue> From(TypeBase source)
            => new SymmetricClosureService(source).Execute(_forward);
        internal static IEnumerable<IValue> To(TypeBase source)
            => new SymmetricClosureService(source).Execute(_backward);

        interface INavigator
        {
            TypeBase Start(IValue feature);
            TypeBase End(IValue feature);
            IValue Combine(IValue startFeature, IValue feature);
        }

        static readonly INavigator _forward = new ForwardNavigator();
        static readonly INavigator _backward = new BackwardNavigator();

        TypeBase Source { get; }
        IEnumerable<IValue> AllFeatures { get; }
        List<TypeBase> _foundTypes;
        List<IValue> _newFeatures;

        SymmetricClosureService(TypeBase source)
        {
            Source = source;
            AllFeatures = source
                .SymmetricFeatureClosure()
                .ToArray();
        }

        IEnumerable<IValue> Combination(INavigator navigator, IValue startFeature = null)
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

        IEnumerable<IValue> Execute(INavigator navigator)
        {
            _foundTypes = new List<TypeBase>();
            _newFeatures = new List<IValue>();

            foreach(var feature in Combination(navigator))
                yield return feature;

            while(_newFeatures.Any())
            {
                var features = _newFeatures;
                _newFeatures = new List<IValue>();
                foreach(var feature in features.SelectMany(f => Combination(navigator, f)))
                    yield return feature;
            }
        }

        sealed class ForwardNavigator : INavigator
        {
            TypeBase INavigator.Start(IValue feature) => feature.Source;
            TypeBase INavigator.End(IValue feature) => feature.ResultType();

            IValue INavigator.Combine(IValue start, IValue end)
                => Feature.Combination.CheckedCreate(start, end);
        }

        sealed class BackwardNavigator : INavigator
        {
            TypeBase INavigator.Start(IValue feature) => feature.ResultType();
            TypeBase INavigator.End(IValue feature) => feature.Source;

            IValue INavigator.Combine(IValue start, IValue end)
                => Feature.Combination.CheckedCreate(end, start);
        }
    }
}