using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
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


        public static Path FindPath(TypeBase source, TypeBase destination, IConversionParameter instance)
        {
            if(source == destination)
                return new Path(source);
            var closure = source.SimpleConversions.ToArray();
            var result = closure.SingleOrDefault(x => x.Destination == destination);
            if(result != null)
                return result;

            var simpleFeatureses = SimpleFeatureses(destination, closure);
            var specificClosure = simpleFeatureses.ToArray();
            result = specificClosure.SingleOrDefault(x => x.Destination == destination);
            if(result != null)
                return result;

            return null;
        }
        static IEnumerable<Path> SimpleFeatureses(TypeBase destination, Path[] closure)
        {
            var featureses = new List<Path>();
            foreach(var path in closure)
                featureses.AddRange(SpecificConversions(destination, path));
            return featureses;
        }
        static IEnumerable<Path> SpecificConversions(TypeBase destination, Path path)
        {
            var reachableTypes = destination
                .ReachableTypes
                .ToArray();
            var simpleFeatures = reachableTypes
                .SelectMany(destinationEntry => path.Destination.GetSpecificConversions(destinationEntry))
                .ToArray();
            var result = simpleFeatures
                .SelectMany(element => Enumerable(destination, path, element))
                .ToArray();
            return result;
        }

        static IEnumerable<Path> Enumerable(TypeBase destination, Path path, ISimpleFeature element)
        {
            var enumerable = element.ResultType()
                .SimpleConversions.Where(e => e.Destination == destination)
                .ToArray();
            var combine = enumerable
                .Combine(new Path(element))
                .ToArray();
            var array = combine
                .Combine(path)
                .ToArray();
            return array;
        }

        public static string DumpObvious(TypeBase source) { return DumpReachable(source, type => type.ConversionElements); }

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

        internal static IEnumerable<Path> FeatureClosure
            (this TypeBase source, Func<TypeBase, IEnumerable<Path>> getElements)
        {
            var identity = new Path(source);
            var types = new TypeBase[0];
            var elements = new Dictionary<TypeBase, Path> {{source, identity}};
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

        static IEnumerable<Path> Combine
            (this IEnumerable<Path> listOfNewContinuations, Path path)
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
                    });
        }
    }
}