using Reni.Feature;

namespace Reni.Type;

static class ConversionService
{
    internal sealed class ClosureService
    {
        TypeBase Source { get; }
        readonly List<TypeBase> FoundTypes = [];
        readonly ValueCache<List<ConversionPath>> NewPathsCache;

        ClosureService(TypeBase source)
        {
            Source = source;
            NewPathsCache = new(() => []);
        }

        internal static IEnumerable<ConversionPath> GetResult(TypeBase source)
            => new ClosureService(source).GetResult();

        IEnumerable<ConversionPath> ExtendPathByOneConversionAndCollect(ConversionPath? startFeature = null)
        {
            var startType = startFeature?.Destination ?? Source;

            var conversionNextStep = startType
                .Conversion.NextStep;
            var newFeatures = conversionNextStep
                .Where(IsRelevantForConversionPathExtension)
                .Select(feature => startFeature + feature)
                .ToArray();

            newFeatures.All(item => item.Source == Source).Assert();

            NewPathsCache.Value.AddRange(newFeatures);
            FoundTypes.AddRange(newFeatures.Select(item => item.Destination));

            return newFeatures;
        }

        bool IsRelevantForConversionPathExtension(IConversion feature)
            => !FoundTypes.Contains(feature.ResultType);

        IEnumerable<ConversionPath> GetResult()
        {
            NewPathsCache.IsValid = false;
            var singularPath = new ConversionPath(Source);
            NewPathsCache.Value.Add(singularPath);
            FoundTypes.Add(singularPath.Destination);
            (singularPath.Source == Source).Assert();
            var results = new List<ConversionPath>
            {
                singularPath
            };

            while(NewPathsCache.IsValid && NewPathsCache.Value.Any())
            {
                var features = NewPathsCache.Value;
                NewPathsCache.IsValid = false;
                var newResults = features.SelectMany(ExtendPathByOneConversionAndCollect).ToArray();
                results.AddRange(newResults);
            }

            return results.ToArray();
        }
    }

    abstract class ConversionProcess : DumpableObject
    {
        TypeBase Source { get; }

        internal ConversionPath? Result
        {
            get
            {
                if(IsDestination(Source))
                    return SimplePath;

                var paths = ClosureService.GetResult(Source);

                var path = paths.FirstOrDefault(path => IsDestination(path.Destination));
                if(path != null)
                    return path;

                (Source.ObjectId == 456).ConditionalBreak(() =>
                    $"""
                     {DestinationDump}
                     {nameof(Source)}={Source.Dump()}
                     {nameof(paths)}={paths.Dump()}
                     """);
                var results = paths
                    .SelectMany(GetForcedConversions)
                    .ToArray();

                (Source.ObjectId == 456).ConditionalBreak(() =>
                    $"""
                     {DestinationDump}
                     {nameof(Source)}={Source.Dump()}
                     {nameof(paths)}={paths.Dump()}
                     """);

                var weight = results.Select(path => path.Weight).Minn;

                return results.Top
                (
                    path => path.Weight == weight
                    , multipleException: dupEx
                );

                 Exception dupEx(IEnumerable<ConversionPath> arg)
                {
                    NotImplementedMethod([arg.ToArray()]);
                    return new InvalidOperationException("Sequence contains more than one matching element");
                }
            }
        }

        ConversionPath SimplePath => new(Source);
        protected ConversionProcess(TypeBase source) => Source = source;

        protected abstract IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left);
        protected abstract bool IsDestination(TypeBase source);

        [UsedImplicitly]
        protected abstract string DestinationDump { get; }
    }

    sealed class GenericConversionProcess : ConversionProcess
    {
        readonly Func<TypeBase, bool> IsDestinationCache;

        public GenericConversionProcess(TypeBase source, Func<TypeBase, bool> isDestination)
            : base(source)
            => IsDestinationCache = isDestination;

        protected override IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left)
            => left + left.Destination.GetForcedConversions(IsDestinationCache);

        protected override bool IsDestination(TypeBase source) => IsDestinationCache(source);

        protected override string DestinationDump => "<multiple Destinations>";
    }

    sealed class ExplicitConversionProcess : ConversionProcess
    {
        TypeBase Destination { get; }

        public ExplicitConversionProcess(TypeBase source, TypeBase destination)
            : base(source)
            => Destination = destination;

        protected override IEnumerable<ConversionPath> GetForcedConversions(ConversionPath left)
            => Destination
                .SymmetricPathsClosureBackwards()
                .SelectMany
                    (right => left + left.Destination.GetForcedConversions(right.Source) + right);

        protected override bool IsDestination(TypeBase source) => source == Destination;
        protected override string DestinationDump => $"{nameof(Destination)}={Destination.Dump()}";
    }

    internal static ConversionPath? FindPath(TypeBase source, TypeBase destination)
        => new ExplicitConversionProcess(source, destination).Result;

    static ConversionPath? FindPath(TypeBase source, Func<TypeBase, bool> isDestination)
        => new GenericConversionProcess(source, isDestination).Result;

    internal static IEnumerable<TDestination> FindPathDestination<TDestination>(TypeBase source)
        where TDestination : TypeBase
    {
        var path = FindPath(source, t => t is TDestination);
        return path == null? [] : [(TDestination)path.Destination];
    }

    internal static IEnumerable<IConversion> ForcedConversions
        (ConversionPath source, ConversionPath destination)
        => source.Destination
            .GetForcedConversions(destination.Source);

    internal static IEnumerable<SearchResult> RemoveLowPriorityResults
        (this IEnumerable<SearchResult> list)
        => list.FrameElementList((a, b) => a.HasHigherPriority(b));

    extension(TypeBase source)
    {
        internal IEnumerable<ConversionPath> CloseRelativeConversions()
        {
            var paths = ClosureService.GetResult(source);
            return paths.Where(path => path.Elements.Any());
        }

        internal IEnumerable<IConversion> SymmetricFeatureClosure()
        {
            var result = RawSymmetricFeatureClosure(source).ToArray();
            result.IsSymmetric().Assert
            (() => result.Select
                (path => new
                    {
                        source = path.Source.OverView.DumpPrintText, destination = path.ResultType.OverView.DumpPrintText
                    }
                )
                .Stringify("\n")
            );
            return result;
        }

        IEnumerable<IConversion> RawSymmetricFeatureClosure()
        {
            var types = new TypeBase[0];
            var newTypes = new[] { source };
            do
            {
                types = types.Union(newTypes).ToArray();
                var newElements = newTypes
                    .SelectMany(type => type.Conversion.Symmetric)
                    .ToArray();
                foreach(var element in newElements)
                    yield return element;

                newTypes = newElements
                    .Select(element => element.ResultType)
                    .Except(types)
                    .ToArray();
            }
            while(newTypes.Any());
        }

        internal IEnumerable<ConversionPath> SymmetricPathsClosure()
            =>
                new[] { new ConversionPath(source) }.Concat
                    (source.Conversion.SymmetricClosure.Select(f => new ConversionPath(f)));

        internal IEnumerable<ConversionPath> SymmetricPathsClosureBackwards
            ()
            =>
                new[] { new ConversionPath(source) }.Concat
                    (SymmetricClosureService.To(source).Select(f => new ConversionPath(f)));
    }

    extension(IEnumerable<IConversion> list)
    {
        internal IEnumerable<TypeBase> Types() => list
            .SelectMany(i => new[] { i.Source, i.ResultType })
            .Distinct();

        internal IEnumerable<IConversion> RemoveCircles
            ()
        {
            var result = new List<IConversion>(list);
            result.AssertPath();
            if(!result.Any())
                return list;
            var source = result.First().Source;
            if(source == result.Last().ResultType)
                return new IConversion[0];

            for(var i = 0; i < result.Count; i++)
            {
                var s = result[i].Source;
                var simpleFeatures =
                    result.Skip(i + 1)
                        .Reverse()
                        .SkipWhile(element => element.Source != s)
                        .ToArray();
                var tailLength = simpleFeatures.Length;
                if(tailLength != 0)
                    result.RemoveRange(i, result.Count - i - tailLength);
                (source == result.First().Source).Assert();
            }

            return result;
        }
    }

    extension(IConversion[] list)
    {
        bool IsSymmetric()
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
    }

    extension(IReadOnlyList<IConversion> elements)
    {
        void AssertPath()
        {
            var features = elements
                .Skip(1)
                .Select
                ((element, i) => new
                {
                    i, result = elements[i].ResultType, next = element.Source
                })
                .Where(item => item.result != item.next)
                .ToArray();
            (!features.Any()).Assert(features.Stringify("\n"));
        }
    }

    extension<T>(IEnumerable<T> list)
    {
        IEnumerable<T> FrameElementList(Func<T, T, bool> isInRelation)
        {
            var l = list.ToArray();
            return l.Where(item => l.All(other => other!.Equals(item) || !isInRelation(other, item)));
        }
    }
}
