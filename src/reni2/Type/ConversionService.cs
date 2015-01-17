using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    static class ConversionService
    {
        internal sealed class Path : DumpableObject, IEquatable<Path>
        {
            internal readonly TypeBase Source;
            internal readonly ISimpleFeature[] Elements;

            internal Path(TypeBase source)
            {
                Source = source;
                Elements = new ISimpleFeature[0];
                Tracer.Assert(Source != null);
            }

            internal Path(params ISimpleFeature[] rawElements)
            {
                Tracer.Assert(rawElements.Any());
                Source = rawElements.First().TargetType;
                Tracer.Assert(Source != null);
                Elements = rawElements.RemoveCycles().ToArray();

                if(Elements.Any())
                    Tracer.Assert(Source == Elements.First().TargetType);

                Tracer.Assert
                    (
                        Types.Count() == Elements.Count() + 1,
                        () => "\n" + Types.Select(t => t.DumpPrintText).Stringify("\n") + "\n****\n" + Dump()
                    );
            }

            IEnumerable<TypeBase> Types
                => Elements
                    .Select(element => element.TargetType)
                    .Concat(new[] {Destination})
                    .Distinct();

            IEnumerable<string> DumpConversions
                => Elements
                    .Select(element => element.Result(Category.Code).Code.DebuggerDumpString)
                .ToArray();

            internal TypeBase Destination => Elements.LastOrDefault()?.ResultType() ?? Source;

            [DisableDump]
            internal bool IsRelativeConversion => Elements.Any() && Elements.All(Extension.IsRelative);

            public static Path operator +(Path a, Path b) => new Path(a.Elements.Concat(b.Elements).ToArray());
            public static IEnumerable<Path> operator +(IEnumerable<Path> a, Path b) => a.Select(left => left + b);
            public static IEnumerable<Path> operator +(Path a, IEnumerable<Path> b) => b.Select(right => a + right);
            public static IEnumerable<Path> operator +(Path a, IEnumerable<ISimpleFeature> b) => b.Select(right => a + right);
            public static Path operator +(ISimpleFeature a, Path b) => new Path(new[] {a}.Concat(b.Elements).ToArray());
            public static Path operator +(Path a, ISimpleFeature b) => new Path(a.Elements.Concat(new[] {b}).ToArray());

            internal Result Execute(Category category)
                => Elements.Aggregate(Source.ArgResult(category), (c, n) => n.Result(category).ReplaceArg(c));

            bool IEquatable<Path>.Equals(Path other)
            {
                if(this == other)
                    return true;
                if(Source != other.Source)
                    return false;
                if(Elements.Length != other.Elements.Length)
                    return false;
                return !Elements.Where((element, index) => element != Elements[index]).Any();
            }
            internal IEnumerable<SearchResult> RelativeSearchResults(Definable definable)
            {
                return Destination
                    .DeclarationsForType(definable)
                    .Select(result => new SearchResult(result, this));
            }
        }

        abstract class ConversionProcess : DumpableObject
        {
            TypeBase Source { get; }
            protected ConversionProcess(TypeBase source) { Source = source; }

            internal Path Result
            {
                get
                {
                    if(IsDestination(Source))
                        return SimplePath;

                    var paths = ClosureService.Result(Source);
                    var others = new List<Path>();

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

            protected abstract IEnumerable<Path> GetForcedConversions(Path left);
            protected abstract bool IsDestination(TypeBase source);

            Path SimplePath => new Path(Source);
        }

        sealed class GenericConversionProcess : ConversionProcess
        {
            readonly Func<TypeBase, bool> _isDestination;

            public GenericConversionProcess(TypeBase source, Func<TypeBase, bool> isDestination)
                : base(source) { _isDestination = isDestination; }

            protected override IEnumerable<Path> GetForcedConversions(Path left)
                => left + left.Destination.GetForcedConversions(_isDestination);
            protected override bool IsDestination(TypeBase source) => _isDestination(source);
        }

        sealed class ExplicitConversionProcess : ConversionProcess
        {
            TypeBase Destination { get; }
            public ExplicitConversionProcess(TypeBase source, TypeBase destination)
                : base(source) { Destination = destination; }

            protected override IEnumerable<Path> GetForcedConversions(Path left)
                => Destination
                    .SymmetricPathsClosureBackwards()
                    .SelectMany(right => left + left.Destination.GetForcedConversions(right.Source) + right);

            protected override bool IsDestination(TypeBase source) => source == Destination;
        }

        internal static Path FindPath(TypeBase source, TypeBase destination)
            => new ExplicitConversionProcess(source, destination).Result;

        static Path FindPath(TypeBase source, Func<TypeBase, bool> isDestination)
            => new GenericConversionProcess(source, isDestination).Result;

        internal static TDestination FindPathDestination<TDestination>(TypeBase source)
            where TDestination : TypeBase
            => (TDestination) FindPath(source, t => t is TDestination)?.Destination;

        internal static IEnumerable<ISimpleFeature> ForcedConversions(Path source, Path destination)
            => source.Destination
                .GetForcedConversions(destination.Source);

        internal static IEnumerable<Path> RelativeConversions(this TypeBase source)
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

        static IEnumerable<ISimpleFeature> RemoveCycles(this IEnumerable<ISimpleFeature> list)
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

        internal static IEnumerable<Path> SymmetricPathsClosure(this TypeBase source)
            => new[] {new Path(source)}.Concat(SymmetricClosureService.From(source).Select(f => new Path(f)));

        internal static IEnumerable<Path> SymmetricPathsClosureBackwards(this TypeBase destination)
            => new[] {new Path(destination)}.Concat(SymmetricClosureService.To(destination).Select(f => new Path(f)));

        internal sealed class ClosureService
        {
            internal static IEnumerable<Path> Result(TypeBase source) => new ClosureService(source).Result();

            static IEnumerable<ISimpleFeature> NextConversionStep(TypeBase source)
                => SymmetricClosureService.From(source).Union(source.StripConversions);

            TypeBase Source { get; }
            List<TypeBase> _foundTypes;
            List<Path> _newPaths;

            ClosureService(TypeBase source) { Source = source; }

            IEnumerable<Path> Combination(Path startFeature = null)
            {
                var startType = Source;
                if(startFeature != null)
                    startType = startFeature.Destination;

                var newFeatures = NextConversionStep(startType)
                    .Where(feature => !_foundTypes.Contains(feature.ResultType()))
                    .Select(feature => startFeature == null ? new Path(feature) : startFeature + feature);
                var result = new List<Path>();
                foreach(var newPath in newFeatures)
                {
                    Tracer.Assert(newPath.Source == Source);
                    AddPath(newPath);
                    result.Add(newPath);
                }
                return result;
            }

            void AddPath(Path newPath)
            {
                if(_newPaths == null)
                    _newPaths = new List<Path>();
                _newPaths.Add(newPath);

                if(_foundTypes == null)
                    _foundTypes = new List<TypeBase>();
                _foundTypes.Add(newPath.Destination);
            }

            IEnumerable<Path> Result()
            {
                _newPaths = null;
                var singularPath = new Path(Source);
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