using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    static class ConversionService
    {
        internal sealed class Path : DumpableObject
        {
            public readonly TypeBase Source;
            public readonly ISimpleFeature[] Elements;
            static readonly IEqualityComparer<Path> _comparer = new PathComparer();

            sealed class PathComparer : DumpableObject, IEqualityComparer<Path>
            {
                public bool Equals(Path x, Path y)
                {
                    if(x.Source != y.Source)
                        return false;
                    if(x.Elements.Length != y.Elements.Length)
                        return false;
                    var @equals = !x.Elements.Where((e, i) => e != y.Elements[i]).Any();
                    return @equals;
                }
                public int GetHashCode(Path obj) => 0;
            }

            public Path(TypeBase source)
            {
                Source = source;
                Elements = new ISimpleFeature[0];
                Tracer.Assert(Source != null);
            }

            Path(params ISimpleFeature[] elements)
            {
                Tracer.Assert(elements.Any());
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
                    .Where(item => item.result != item.next);
                Tracer.Assert(!features.Any(), features.Stringify("\n"));
                Source = elements.First().TargetType;
                Elements = elements;
                Tracer.Assert(Source != null);
            }

            public TypeBase Destination => Elements.LastOrDefault()?.ResultType() ?? Source;
            public static IEqualityComparer<Path> Comparer { get { return _comparer; } }

            protected override string GetNodeDump()
            {
                return Elements
                    .Select(element => element.TargetType.DumpPrintText + " -> ")
                    .Stringify("")
                    + Destination.DumpPrintText;
            }

            public static Path CreateFromList(TypeBase source, TypeBase destination, ISimpleFeature[] features)
            {
                return (features.Length + 1)
                    .Select()
                    .Select(length => CreateFromList(length, source, destination, features))
                    .First(p => p != null);
            }

            static Path CreateFromList(int length, TypeBase source, TypeBase destination, ISimpleFeature[] features)
            {
                if(source == destination)
                    return new Path(source);

                if(length == 0)
                    return null;

                return features
                    .Where(f => f.TargetType == source)
                    .Select(f => PrefixIfNotNull(f, CreateFromList(length - 1, f.ResultType(), destination, features)))
                    .FirstOrDefault(p => p != null);
            }

            static Path PrefixIfNotNull(ISimpleFeature a, Path b) => b == null ? null : a + b;

            public static Path operator +(Path a, Path b) => new Path(a.Elements.Concat(b.Elements).ToArray());
            public static IEnumerable<Path> operator +(IEnumerable<Path> a, Path b) => a.Select(left => left + b);
            public static IEnumerable<Path> operator +(Path a, IEnumerable<Path> b) => b.Select(right => a + right);
            public static IEnumerable<Path> operator +(Path a, IEnumerable<ISimpleFeature> b) => b.Select(right => a + right);
            public static Path operator +(ISimpleFeature a, Path b) => new Path(new[] {a}.Concat(b.Elements).ToArray());
            public static Path operator +(Path a, ISimpleFeature b) => new Path(a.Elements.Concat(new[] {b}).ToArray());

            Path CheckedAppend(Path other) => Destination == other.Source ? this + other : null;

            internal IEnumerable<Path> CheckedAppend(IEnumerable<Path> b)
                => b.SelectMany(right => CheckedAppend(right).NullableToArray());
            internal Result Execute(Category category)
                => Elements.Aggregate(Source.ArgResult(category), (c, n) => n.Result(category).ReplaceArg(c));
        }

        public static Path FindPath(TypeBase source, TypeBase destination)
        {
            var sourcePaths = new[] {new Path(source)};
            if(source == destination)
                return sourcePaths.Single();

            var destinationPaths = destination.PrefixSymmetricPathsClosure().ToArray();
            var result = destinationPaths.SingleOrDefault(p => p.Source == source);
            if(result != null)
                return result;

            var sourceAndClosurePaths = sourcePaths.Union(source.SymmetricPathsClosure(), Path.Comparer)
                .ToArray();

            var stripPaths = sourceAndClosurePaths
                .SelectMany(path => path + path.Destination.StripConversions)
                .ToArray();

            result = stripPaths.CheckedAppend(destinationPaths).SingleOrDefault();
            if(result != null)
                return result;

            var doubleStriped = stripPaths
                .SelectMany(path => path + path.Destination.SymmetricPathsClosure())
                .SelectMany(path => path + path.Destination.StripConversions)
                ;

            Tracer.Assert(!doubleStriped.Any());

            var results = sourceAndClosurePaths.Union(stripPaths, Path.Comparer)
                .SelectMany(left => destinationPaths.SelectMany(right => left + ForcedConversions(left, right) + right))
                .ToArray();

            return results.SingleOrDefault();
        }

        public static Path FindPath<TDestination>(TypeBase source)
        {
            var sourcePaths = new[] {new Path(source)};
            if(source is TDestination)
                return sourcePaths.Single();

            sourcePaths = sourcePaths.Union(source.SymmetricPathsClosure(), Path.Comparer)
                .ToArray();

            var result = sourcePaths.SingleOrDefault(path => path.Destination is TDestination);
            if(result != null)
                return result;

            var stripPaths = sourcePaths
                .SelectMany(path => path + path.Destination.StripConversions)
                .ToArray();

            var stripPathsFeaturePathClosure = stripPaths
                .SelectMany(path => path + path.Destination.SymmetricPathsClosure())
                .ToArray();

            result = stripPathsFeaturePathClosure.SingleOrDefault(path => path.Destination is TDestination);
            if(result != null)
                return result;

            var doubleStriped = stripPathsFeaturePathClosure
                .SelectMany(path => path + path.Destination.StripConversions)
                ;

            Tracer.Assert(!doubleStriped.Any());

            Dumpable.NotImplementedFunction(source);
            return null;
        }

        internal static IEnumerable<ISimpleFeature> ForcedConversions(Path source, Path destination)
            => source.Destination
                .GetForcedConversions(destination.Source);

        static IEnumerable<T> NullableToArray<T>(this T target) => Equals(target, default(T)) ? new T[0] : new[] {target};

        public static string DumpObvious(TypeBase source) { return DumpReachable(source, type => type.SymmetricConversions); }

        static string DumpReachable(TypeBase source, Func<TypeBase, IEnumerable<ISimpleFeature>> getConversionElements)
        {
            return source.ReachableTypes(getConversionElements).Select(t => t.NodeDump).Stringify("\n");
        }

        static IEnumerable<TypeBase> ReachableTypes
            (this TypeBase source, Func<TypeBase, IEnumerable<ISimpleFeature>> getConversionElements)
        {
            var types = new TypeBase[0];
            var newTypes = new[] {source};
            do
            {
                types = types.Union(newTypes).ToArray();
                newTypes = newTypes
                    .SelectMany(type => getConversionElements(type).Select(c => c.ResultType()))
                    .Where(type => !types.Contains(type))
                    .ToArray();
            } while(newTypes.Any());
            return types;
        }

        internal static IEnumerable<ISimpleFeature> SymmetricFeatureClosure(this TypeBase source)
        {
            var result = RawSymmetricFeatureClosure(source).ToArray();
            Tracer.Assert
                (
                    result.IsSymmetric(),
                    result.Select
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

        static bool IsSymmetric(ISimpleFeature left, ISimpleFeature right)
            => left.ResultType() == right.TargetType
                && right.ResultType() == left.TargetType;

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

        static IEnumerable<Path> PrefixSymmetricPathsClosure(this TypeBase destination)
        {
            var features = destination
                .SymmetricFeatureClosure()
                .ToArray();
            return features
                .Select(f => f.TargetType)
                .Distinct()
                .Select(t => Path.CreateFromList(t, destination, features));
        }

        internal static IEnumerable<Path> SymmetricPathsClosure(this TypeBase source)
        {
            var features = source
                .SymmetricFeatureClosure()
                .ToArray();
            return features
                .Select(f => f.ResultType())
                .Distinct()
                .Select(t => Path.CreateFromList(source, t, features));
        }

        static IEnumerable<Path> CheckedAppend(this IEnumerable<Path> a, IEnumerable<Path> b)
            => a.SelectMany(left => left.CheckedAppend(b));
    }
}