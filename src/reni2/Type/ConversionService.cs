using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
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
                : base(source) { _isDestination = isDestination; }

            protected override IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left)
                => left + left.Destination.GetForcedConversions(_isDestination);
            protected override bool IsDestination(TypeBase source) => _isDestination(source);
        }

        sealed class ExplicitConversionProcess : ConversionProcess
        {
            TypeBase Destination { get; }
            public ExplicitConversionProcess(TypeBase source, TypeBase destination)
                : base(source) { Destination = destination; }

            protected override IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left)
                => Destination
                    .SymmetricPathsClosureBackwards()
                    .SelectMany(right => left + left.Destination.GetForcedConversions(right.Source) + right);

            protected override bool IsDestination(TypeBase source) => source == Destination;
        }

        internal static ConversionPath FindPath(TypeBase source, TypeBase destination)
            => new ExplicitConversionProcess(source, destination).Result;

        static ConversionPath FindPath(TypeBase source, Func<TypeBase, bool> isDestination)
            => new GenericConversionProcess(source, isDestination).Result;

        internal static TDestination FindPathDestination<TDestination>(TypeBase source)
            where TDestination : TypeBase
            => (TDestination) FindPath(source, t => t is TDestination)?.Destination;

        internal static IEnumerable<ISimpleFeature> ForcedConversions(ConversionPath source, ConversionPath destination)
            => source.Destination
                .GetForcedConversions(destination.Source);

        internal static IEnumerable<ConversionPath> RelativeConversions(this TypeBase source)
            => ClosureService
                .Result(source)
                .Where(path => path.IsRelativeConversion);

        internal static IEnumerable<ISimpleFeature> SymmetricFeatureClosure(this TypeBase source)
        {
            var result = RawSymmetricFeatureClosure(source).ToArray();
            Tracer.Assert
                (
                    result.IsSymmetric(),
                    () => result.Select
                        (
                            path => new
                            {
                                source = path.TargetType.DumpPrintText,
                                destination = path.ResultType().DumpPrintText
                            }
                        )
                        .Stringify("\n")
                );
            return result;
        }

        static bool IsSymmetric(this ISimpleFeature[] list)
        {
            if(!list.Any())
                return true;
            var x = list
                .Types()
                .Select(t => t.RawSymmetricFeatureClosure().Types().OrderBy(f => f.ObjectId).Count())
                .ToArray();
            var y = x.Distinct().ToArray();
            return y.Length == 1 && x.Length == y.Single();
        }

        internal static IEnumerable<TypeBase> Types(this IEnumerable<ISimpleFeature> list)
        {
            return list
                .SelectMany(i => new[] {i.TargetType, i.ResultType()})
                .Distinct();
        }

        static void AssertPath(this IReadOnlyList<ISimpleFeature> elements)
        {
            var features = elements
                .Skip(1)
                .Select
                (
                    (element, i) => new
                    {
                        i,
                        result = elements[i].ResultType(),
                        next = element.TargetType
                    })
                .Where(item => item.result != item.next)
                .ToArray();
            Tracer.Assert(!features.Any(), features.Stringify("\n"));
        }

        internal static IEnumerable<ISimpleFeature> RemoveCircles(this IEnumerable<ISimpleFeature> list)
        {
            var result = new List<ISimpleFeature>(list);
            result.AssertPath();
            if(!result.Any())
                return list;
            var source = result.First().TargetType;
            if(source == result.Last().ResultType())
                return new ISimpleFeature[0];

            for(var i = 0; i < result.Count; i++)
            {
                var s = result[i].TargetType;
                var simpleFeatures = result.Skip(i + 1).Reverse().SkipWhile(element => element.TargetType != s).ToArray();
                var tailLength = simpleFeatures.Count();
                if(tailLength != 0)
                    result.RemoveRange(i, result.Count - i - tailLength);
                Tracer.Assert(source == result.First().TargetType);
            }

            return result;
        }

        static IEnumerable<ISimpleFeature> RawSymmetricFeatureClosure(this TypeBase source)
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
            => new[] {new ConversionPath(source)}.Concat(SymmetricClosureService.From(source).Select(f => new ConversionPath(f)));

        internal static IEnumerable<ConversionPath> SymmetricPathsClosureBackwards(this TypeBase destination)
            => new[] {new ConversionPath(destination)}.Concat(SymmetricClosureService.To(destination).Select(f => new ConversionPath(f)));

        internal static IEnumerable<SearchResult> RemoveLowPriorityResults(this IEnumerable<SearchResult> list)
            => list.FrameElementList((a, b) => a.HasHigherPriority(b));

        static IEnumerable<T> FrameElementList<T>(this IEnumerable<T> list, Func<T, T, bool> isInRelation)
        {
            var l = list.ToArray();
            return l.Where(item => l.All(other => other.Equals(item) || !isInRelation(other, item)));
        }

        internal sealed class ClosureService
        {
            internal static IEnumerable<ConversionPath> Result(TypeBase source) => new ClosureService(source).Result();

            static IEnumerable<ISimpleFeature> NextConversionStep(TypeBase source)
                => SymmetricClosureService.From(source).Union(source.StripConversions);

            TypeBase Source { get; }
            List<TypeBase> _foundTypes;
            List<ConversionPath> _newPaths;

            ClosureService(TypeBase source) { Source = source; }

            IEnumerable<ConversionPath> Combination(ConversionPath startFeature = null)
            {
                var startType = Source;
                if(startFeature != null)
                    startType = startFeature.Destination;

                var newFeatures = NextConversionStep(startType)
                    .Where(feature => !_foundTypes.Contains(feature.ResultType()))
                    .Select(feature => startFeature == null ? new ConversionPath(feature) : startFeature + feature);
                var result = new List<ConversionPath>();
                foreach(var newPath in newFeatures)
                {
                    Tracer.Assert(newPath.Source == Source);
                    AddPath(newPath);
                    result.Add(newPath);
                }
                return result;
            }

            void AddPath(ConversionPath newPath)
            {
                if(_newPaths == null)
                    _newPaths = new List<ConversionPath>();
                _newPaths.Add(newPath);

                if(_foundTypes == null)
                    _foundTypes = new List<TypeBase>();
                _foundTypes.Add(newPath.Destination);
            }

            IEnumerable<ConversionPath> Result()
            {
                _newPaths = null;
                var singularPath = new ConversionPath(Source);
                AddPath(singularPath);
                Tracer.Assert(singularPath.Source == Source);
                yield return singularPath;

                while(_newPaths != null && _newPaths.Any())
                {
                    var features = _newPaths;
                    _newPaths = null;
                    foreach(var newPath in (features.SelectMany(Combination).ToArray()))
                    {
                        Tracer.Assert(newPath.Source == Source);
                        yield return newPath;
                    }
                }
            }
        }
    }
}