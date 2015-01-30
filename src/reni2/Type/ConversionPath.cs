using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    internal sealed class ConversionPath : DumpableObject, IEquatable<ConversionPath>
    {
        internal readonly TypeBase Source;
        internal readonly ISimpleFeature[] Elements;

        internal ConversionPath(TypeBase source)
        {
            Source = source;
            Elements = new ISimpleFeature[0];
            Tracer.Assert(Source != null);
        }

        internal ConversionPath(params ISimpleFeature[] rawElements)
        {
            Tracer.Assert(rawElements.Any());
            Source = rawElements.First().TargetType;
            Tracer.Assert(Source != null);
            Elements = rawElements.RemoveCircles().ToArray();

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

        public static ConversionPath operator +(ConversionPath a, ConversionPath b) => new ConversionPath(a.Elements.Concat(b.Elements).ToArray());
        public static IEnumerable<ConversionPath> operator +(IEnumerable<ConversionPath> a, ConversionPath b) => a.Select(left => left + b);
        public static IEnumerable<ConversionPath> operator +(ConversionPath a, IEnumerable<ConversionPath> b) => b.Select(right => a + right);
        public static IEnumerable<ConversionPath> operator +(ConversionPath a, IEnumerable<ISimpleFeature> b) => b.Select<ISimpleFeature, ConversionPath>(right => a + right);
        public static ConversionPath operator +(ISimpleFeature a, ConversionPath b) => new ConversionPath(new[] {a}.Concat(b.Elements).ToArray());
        public static ConversionPath operator +(ConversionPath a, ISimpleFeature b) => new ConversionPath(a.Elements.Concat(new[] {b}).ToArray());

        internal Result Execute(Category category)
            => Elements.Aggregate(Source.ArgResult(category), (c, n) => n.Result(category).ReplaceArg(c));

        bool IEquatable<ConversionPath>.Equals(ConversionPath other)
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
            var declarationsForType = Destination
                .DeclarationsForType(definable);
            return declarationsForType
                .Select(result => new SearchResult(result, this));
        }

        internal bool HasHigherPriority(ConversionPath other)
        {
            if(Elements.Length < other.Elements.Length)
                return true;

            if(Elements.Length > other.Elements.Length)
                return false;

            NotImplementedMethod(other);
            return true;
        }
    }
}