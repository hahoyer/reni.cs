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
    sealed class ConversionPath
        : DumpableObject
            , IEquatable<ConversionPath>
            , ResultCache.IResultProvider
    {
        static int _nextObjectId;

        internal readonly TypeBase Source;
        internal readonly IValueFeature[] Elements;

        internal ConversionPath()
            : base(_nextObjectId++) { }

        internal ConversionPath(TypeBase source)
            : this()
        {
            Source = source;
            Elements = new IValueFeature[0];
            Tracer.Assert(IsValid);
        }

        internal bool IsValid => Source != null;

        internal ConversionPath(params IValueFeature[] rawElements)
            : this()
        {
            Tracer.Assert(rawElements.Any());
            Source = rawElements.First().TargetType;
            Tracer.Assert(IsValid);
            Elements = rawElements.RemoveCircles().ToArray();

            if(Elements.Any())
                Tracer.Assert(Source == Elements.First().TargetType);

            if(false)
                Tracer.Assert
                    (
                        Types.Count() == Elements.Count() + 1,
                        () =>
                            "\n" + Types.Select(t => t.DumpPrintText).Stringify("\n") + "\n****\n"
                                + Dump()
                    );
            StopByObjectId(-284);
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
        internal bool IsCloseRelativeConversion
            => Elements.Any() && Elements.All(Feature.Extension.IsCloseRelative);

        public static ConversionPath operator +(ConversionPath a, ConversionPath b)
            => new ConversionPath(a.Elements.Concat(b.Elements).ToArray());

        public static IEnumerable<ConversionPath> operator +(
            IEnumerable<ConversionPath> a,
            ConversionPath b)
            => a.Select(left => left + b);

        public static IEnumerable<ConversionPath> operator +(
            ConversionPath a,
            IEnumerable<ConversionPath> b)
            => b.Select(right => a + right);

        public static IEnumerable<ConversionPath> operator +(
            ConversionPath a,
            IEnumerable<IValueFeature> b)
            => b.Select(right => a + right);

        public static ConversionPath operator +(IValueFeature a, ConversionPath b)
            => new ConversionPath(new[] {a}.Concat(b.Elements).ToArray());

        public static ConversionPath operator +(ConversionPath a, IValueFeature b)
            => new ConversionPath(a.Elements.Concat(new[] {b}).ToArray());

        internal Result Execute(Category category)
            =>
                Elements.Aggregate
                    (
                        new ResultCache(this),
                        (c, n) => n.Result(category.Typed).ReplaceArg(c)) & category;

        object ResultCache.IResultProvider.Target => this;

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

        internal IEnumerable<SearchResult> CloseRelativeSearchResults(Definable tokenClass)
            => Destination
                .DeclarationsForType(tokenClass)
                .Select(result => new SearchResult(result, this));

        internal bool HasHigherPriority(ConversionPath other)
        {
            if(Destination is PointerType && !(other.Destination is PointerType))
                return true;

            if(other.Destination is PointerType && !(Destination is PointerType))
                return false;

            if(Elements.Length < other.Elements.Length)
                return true;

            if(Elements.Length > other.Elements.Length)
                return false;

            NotImplementedMethod(other);
            return true;
        }

        Result ResultCache.IResultProvider.Execute(Category category, Category pendingCategory)
        {
            Tracer.Assert(pendingCategory.IsNone);
            return Source.ArgResult(category);
        }
    }
}