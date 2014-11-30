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
        internal sealed class Path : DumpableObject
        {
            public readonly TypeBase Source;
            public readonly ISimpleFeature[] Elements;

            public Path(TypeBase source)
            {
                Source = source;
                Elements = new ISimpleFeature[0];
                Tracer.Assert(Source != null);
            }

            public Path(params ISimpleFeature[] elements)
            {
                Tracer.Assert(elements.Any());
                Tracer.Assert(!elements.Skip(1).Where((element, i) => element.ResultType() != elements[i].TargetType).Any());
                Source = elements.Last().TargetType;
                Elements = elements;
                Tracer.Assert(Source != null);
            }

            public TypeBase Destination
            {
                get
                {
                    var result = Elements.FirstOrDefault();
                    if(result != null)
                        return result.ResultType();
                    return Source;
                }
            }

            protected override string GetNodeDump()
            {
                return Elements
                    .Select(element => element.ResultType().DumpPrintText + " <== ")
                    .Stringify("")
                    + Source.DumpPrintText;
            }
        }

        public static Path FindPath(TypeBase source, TypeBase destination)
        {
            var result = FindPath(source, t => t == destination, () => destination.GetReflexiveConversionPaths().ToArray());
            if(result != null)
                return result;

            var conversions = source.GetReflexiveConversionPaths().ToArray();

            var stripConversions = conversions.SelectMany
                (
                    path => path
                        .Destination
                        .GetStripConversion()
                        .NullableToArray()
                        .SelectMany(conversion => destination.Combine(conversion, path))
                )
                .ToArray();

            var destinations = destination.GetReflexiveConversionPaths().ToArray();

            var conversionsWithStrip = conversions.Concat(stripConversions).ToArray();
            return conversionsWithStrip
                .SelectMany(outer => destinations.SelectMany(inner => ForcedConversions(outer, inner)))
                .SingleOrDefault(x => x.Destination == destination);
        }

        public static Path FindPath<TDestination>(TypeBase source)
        {
            return FindPath(source, destination => destination is TDestination);
        }

        static Path FindPath(TypeBase source, Func<TypeBase, bool> isDestination, Func<Path[]> getDestinations = null)
        {
            if(isDestination(source))
                return new Path(source);

            var conversions = source.GetReflexiveConversionPaths().ToArray();
            var result = conversions.SingleOrDefault(x => isDestination(x.Destination));
            if(result != null)
                return result;

            var stripConversions = conversions
                .SelectMany
                (
                    path => path
                        .Destination
                        .GetStripConversion()
                        .NullableToArray()
                        .SelectMany(conversion => Combine(isDestination, conversion, path))
                )
                .ToArray();

            var resultWithStrip = stripConversions.SingleOrDefault(x => isDestination(x.Destination));
            if(resultWithStrip != null || getDestinations == null)
                return resultWithStrip;

            var conversionsWithStrip = conversions.Concat(stripConversions).ToArray();
            if(!conversionsWithStrip.Any())
                return null;

            var destinations = getDestinations();
            if(!destinations.Any())
                return null;

            return conversionsWithStrip
                .SelectMany(outer => destinations.SelectMany(inner => ForcedConversions(outer, inner)))
                .SingleOrDefault(x => isDestination(x.Destination));
        }

        static IEnumerable<Path> ForcedConversions(Path source, Path destination)
        {
            return source
                .Destination
                .GetForcedConversions(destination.Source)
                .Select(conversion => Combine(source, conversion, destination));
        }

        static Path Combine(Path source, ISimpleFeature conversion, Path destination)
        {
            var simpleFeatures = destination.Elements
                .Concat(new[] {conversion})
                .Concat(source.Elements);
            return new Path(simpleFeatures.ToArray());
        }

        static IEnumerable<Path> Combine(this TypeBase destination, ISimpleFeature element, Path path)
        {
            return element
                .ResultType()
                .GetReflexiveConversionPaths()
                .Where(e => e.Destination == destination)
                .Combine(new Path(element))
                .Combine(path)
                ;
        }

        static IEnumerable<Path> Combine(Func<TypeBase, bool> isDestination, ISimpleFeature element, Path path)
        {
            return element
                .ResultType()
                .GetReflexiveConversionPaths()
                .Where(e => isDestination(e.Destination))
                .Combine(new Path(element))
                .Combine(path)
                ;
        }
        static IEnumerable<T> NullableToArray<T>(this T target) { return Equals(target, default(T)) ? new T[0] : new[] {target}; }

        public static string DumpObvious(TypeBase source) { return DumpReachable(source, type => type.ReflexiveConversions); }

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

        static IEnumerable<Path> FeatureClosure
            (this TypeBase source, Func<TypeBase, IEnumerable<Path>> getElements)
        {
            var identity = new Path(source);
            var types = new TypeBase[0];
            var elements = new Dictionary<TypeBase, Path>
            {
                {source, identity}
            };
            yield return identity;

            var newTypes = new[] {source};
            do
            {
                types = types.Union(newTypes).ToArray();
                var newElements = newTypes
                    .SelectMany(type => getElements(type).Combine(elements[type]))
                    .Where(element => !types.Contains(element.Destination))
                    .ToArray();
                foreach(var element in newElements)
                {
                    yield return element;
                    elements.Add(element.Destination, element);
                }

                newTypes = newElements
                    .Select(element => element.Destination)
                    .ToArray();
            } while(newTypes.Any());
        }

        static IEnumerable<Path> Combine(this IEnumerable<Path> listOfNewContinuations, Path path)
        {
            return listOfNewContinuations.Select
                (
                    newPath =>
                    {
                        Tracer.Assert
                            (
                                path.Destination == newPath.Source,
                                () => "\npath=" + path.NodeDump + "\nnewPath=" + newPath.NodeDump
                            );
                        return new Path(newPath.Elements.Concat(path.Elements).ToArray());
                    }
                )
                .ToArray();
        }

        static IEnumerable<Path> GetReflexiveConversionPaths(this TypeBase target)
        {
            return target.FeatureClosure
                (
                    type => type
                        .ReflexiveConversions
                        .Select(element => new Path(element))
                )
                .ToArray()
                ;
        }
    }
}