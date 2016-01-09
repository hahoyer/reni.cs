using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Feature;

namespace Reni.Type
{
    static class ConversionService
    {
        abstract class ConversionProcess : DumpableObject
        {
            TypeBase Source { get; }
            protected ConversionProcess(TypeBase source) { Source = source; }

            internal ConversionPath Result
            {
                get
                {
                    if(IsDestination(Source))
                        return SimplePath;

                    var paths = ClosureService.Result(Source);
                    if(paths == null)
                        return new ConversionPath();

                    var others = new List<ConversionPath>();

                    foreach(var path in paths)
                    {
                        if(IsDestination(path.Destination))
                            return path;
                        others.Add(path);
                    }

                    var results = others
                        .SelectMany(GetForcedConversions)
                        .ToArray();

                    var length = results.Min(path => (int?) path.Elements.Length);
                    return results.SingleOrDefault(path => path.Elements.Length == length);
                }
            }

            protected abstract IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left);
            protected abstract bool IsDestination(TypeBase source);

            ConversionPath SimplePath => new ConversionPath(Source);
        }

        sealed class GenericConversionProcess : ConversionProcess
        {
            readonly Func<TypeBase, bool> _isDestination;

            public GenericConversionProcess(TypeBase source, Func<TypeBase, bool> isDestination)
                : base(source)
            {
                _isDestination = isDestination;
            }

            protected override IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left)
                => left + left.Destination.GetForcedConversions(_isDestination);

            protected override bool IsDestination(TypeBase source) => _isDestination(source);
        }

        sealed class ExplicitConversionProcess : ConversionProcess
        {
            TypeBase Destination { get; }

            public ExplicitConversionProcess(TypeBase source, TypeBase destination)
                : base(source)
            {
                Destination = destination;
            }

            protected override IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left)
                => Destination
                    .SymmetricPathsClosureBackwards()
                    .SelectMany
                    (right => left + left.Destination.GetForcedConversions(right.Source) + right);

            protected override bool IsDestination(TypeBase source) => source == Destination;
        }

        internal static ConversionPath FindPath(TypeBase source, TypeBase destination)
            => new ExplicitConversionProcess(source, destination).Result;

        static ConversionPath FindPath(TypeBase source, Func<TypeBase, bool> isDestination)
            => new GenericConversionProcess(source, isDestination).Result;

        internal static IEnumerable<TDestination> FindPathDestination<TDestination>(TypeBase source)
            where TDestination : TypeBase
        {
            var path = FindPath(source, t => t is TDestination);
            if(path == null)
                return Enumerable.Empty<TDestination>();
            return path.IsValid ? new[] {(TDestination) path.Destination} : null;
        }

        internal static IEnumerable<IConversion> ForcedConversions
            (ConversionPath source, ConversionPath destination)
            => source.Destination
                .GetForcedConversions(destination.Source);

        internal static IEnumerable<ConversionPath> CloseRelativeConversions(this TypeBase source)
            => ClosureService
                .Result(source)
                .Where(path => path.Elements.Any());

        internal static IEnumerable<IConversion> SymmetricFeatureClosure(this TypeBase source)
        {
            var result = RawSymmetricFeatureClosure(source).ToArray();
            Tracer.Assert
                (
                    result.IsSymmetric(),
                    () => result.Select
                        (
                            path => new
                            {
                                source = path.Source.DumpPrintText,
                                destination = path.ResultType().DumpPrintText
                            }
                        )
                        .Stringify("\n")
                );
            return result;
        }

        static bool IsSymmetric(this IConversion[] list)
        {
            if(!list.Any())
                return true;
            var x = list
                .Types()
                .Select
                (t => t.RawSymmetricFeatureClosure().Types().OrderBy(f => f.ObjectId).Count())
                .ToArray();
            var y = x.Distinct().ToArray();
            return y.Length == 1 && x.Length == y.Single();
        }

        internal static IEnumerable<TypeBase> Types(this IEnumerable<IConversion> list)
        {
            return list
                .SelectMany(i => new[] {i.Source, i.ResultType()})
                .Distinct();
        }

        static void AssertPath(this IReadOnlyList<IConversion> elements)
        {
            var features = elements
                .Skip(1)
                .Select
                (
                    (element, i) => new
                    {
                        i,
                        result = elements[i].ResultType(),
                        next = element.Source
                    })
                .Where(item => item.result != item.next)
                .ToArray();
            //Tracer.Assert(!features.Any(), features.Stringify("\n"));
        }

        internal static IEnumerable<IConversion> RemoveCircles
            (this IEnumerable<IConversion> list)
        {
            var result = new List<IConversion>(list);
            result.AssertPath();
            if(!result.Any())
                return list;
            var source = result.First().Source;
            if(source == result.Last().ResultType())
                return new IConversion[0];

            for(var i = 0; i < result.Count; i++)
            {
                var s = result[i].Source;
                var simpleFeatures =
                    result.Skip(i + 1)
                        .Reverse()
                        .SkipWhile(element => element.Source != s)
                        .ToArray();
                var tailLength = simpleFeatures.Count();
                if(tailLength != 0)
                    result.RemoveRange(i, result.Count - i - tailLength);
                Tracer.Assert(source == result.First().Source);
            }

            return result;
        }

        static IEnumerable<IConversion> RawSymmetricFeatureClosure(this TypeBase source)
        {
            var types = new TypeBase[0];
            var newTypes = new[] {source};
            do
            {
                types = types.Union(newTypes).ToArray();
                var newElements = newTypes
                    .SelectMany(type => type.SymmetricConversions)
                    .ToArray();
                foreach(var element in newElements)
                    yield return element;

                newTypes = newElements
                    .Select(element => element.ResultType())
                    .Except(types)
                    .ToArray();
            } while(newTypes.Any());
        }

        internal static IEnumerable<ConversionPath> SymmetricPathsClosure(this TypeBase source)
            =>
                new[] {new ConversionPath(source)}.Concat
                    (source.SymmetricClosureConversions.Select(f => new ConversionPath(f)));

        internal static IEnumerable<ConversionPath> SymmetricPathsClosureBackwards
            (this TypeBase destination)
            =>
                new[] {new ConversionPath(destination)}.Concat
                    (SymmetricClosureService.To(destination).Select(f => new ConversionPath(f)));

        internal static IEnumerable<SearchResult> RemoveLowPriorityResults
            (this IEnumerable<SearchResult> list)
            => list.FrameElementList((a, b) => a.HasHigherPriority(b));

        static IEnumerable<T> FrameElementList<T>
            (this IEnumerable<T> list, Func<T, T, bool> isInRelation)
        {
            var l = list.ToArray();
            return l.Where(item => l.All(other => other.Equals(item) || !isInRelation(other, item)));
        }

        internal sealed class ClosureService
        {
            internal static IEnumerable<ConversionPath> Result(TypeBase source)
                => new ClosureService(source).Result();

            TypeBase Source { get; }
            readonly List<TypeBase> _foundTypes = new List<TypeBase>();
            readonly ValueCache<List<ConversionPath>> _newPathsCache;

            ClosureService(TypeBase source)
            {
                Source = source;
                _newPathsCache = new ValueCache<List<ConversionPath>>
                    (() => new List<ConversionPath>());
            }

            IEnumerable<ConversionPath> Combination(ConversionPath startFeature = null)
            {
                var startType = Source;
                if(startFeature != null)
                    startType = startFeature.Destination;

                var nextConversionStep = startType.NextConversionStep.ToArray();
                if(nextConversionStep.Any(item => item.ResultType() == null))
                    return null;

                var newFeatures = nextConversionStep
                    .Where(feature => !_foundTypes.Contains(feature.ResultType()))
                    .Select
                    (
                        feature =>
                            startFeature == null
                                ? new ConversionPath(feature)
                                : startFeature + feature
                    )
                    .ToArray();

                Tracer.Assert(newFeatures.All(item => item.Source == Source));

                _newPathsCache.Value.AddRange(newFeatures);
                _foundTypes.AddRange(newFeatures.Select(item => item.Destination));

                return newFeatures;
            }

            IEnumerable<ConversionPath> Result()
            {
                _newPathsCache.IsValid = false;
                var singularPath = new ConversionPath(Source);
                _newPathsCache.Value.Add(singularPath);
                _foundTypes.Add(singularPath.Destination);
                Tracer.Assert(singularPath.Source == Source);
                var result = new List<ConversionPath>
                {
                    singularPath
                };

                while(_newPathsCache.IsValid && _newPathsCache.Value.Any())
                {
                    var features = _newPathsCache.Value;
                    _newPathsCache.IsValid = false;

                    foreach(var newPaths in features.Select(Combination))
                    {
                        if(newPaths == null)
                            return null;
                        result.AddRange(newPaths);
                    }
                }
                return result.ToArray();
            }
        }
    }
}