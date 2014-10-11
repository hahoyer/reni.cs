using System;
using System.Collections.Generic;
using System.Linq;
using hw.Helper;
using Reni.Feature;

namespace Reni.Type
{
    static class ConversionService
    {
        public static ISimpleFeature[] FindPath(TypeBase source, TypeBase destination)
        {
            if(source == destination)
                return new ISimpleFeature[0];
            var closure = Closure(source);
            return closure.SingleOrDefault(x => x.First().ResultType() == destination);
        }

        public static string DumpObvious(TypeBase source)
        {
            return source.ReachableTypes().Select(t => t.NodeDump).Stringify("\n");
        }

        static IEnumerable<TypeBase> ReachableTypes(this TypeBase source)
        {
            var types = new TypeBase[0];
            var newTypes = new[] {source};
            do
            {
                types = types.Union(newTypes).ToArray();
                newTypes = newTypes
                    .SelectMany(type => type.ConversionElements.Select(c => c.ResultType()))
                    .Where(type => !types.Contains(type))
                    .ToArray();
            } while(newTypes.Any());
            return types;
        }

        static IEnumerable<ISimpleFeature[]> Closure(TypeBase source)
        {
            var types = new TypeBase[0];
            var elements = new Dictionary<TypeBase, IEnumerable<ISimpleFeature>> {{source, new ISimpleFeature[0]}};
            var newTypes = new[] {source};
            do
            {
                types = types.Union(newTypes).ToArray();
                var newElements = newTypes
                    .SelectMany(type => Combine(type.ConversionElements, elements[type]))
                    .Where(element => !types.Contains(element.First().ResultType()))
                    .ToArray();
                foreach(var element in newElements)
                {
                    yield return element;
                    elements.Add(element.First().ResultType(), element);
                }

                newTypes = newElements
                    .Select(element => element.First().ResultType())
                    .ToArray();
            } while(newTypes.Any());
        }

        static IEnumerable<ISimpleFeature[]> Combine
            (IEnumerable<ISimpleFeature> listOfNewContinuations, IEnumerable<ISimpleFeature> path)
        {
            return listOfNewContinuations.Select(element => new[] {element}.Concat(path).ToArray());
        }
    }
}