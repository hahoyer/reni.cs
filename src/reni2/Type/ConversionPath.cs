using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
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
        internal readonly IConversion[] Elements;

        internal ConversionPath()
            : base(_nextObjectId++) { }

        internal ConversionPath(TypeBase source)
            : this()
        {
            Source = source;
            Elements = new IConversion[0];
            Tracer.Assert(IsValid);
        }

        [DisableDump]
        internal bool IsValid => Source != null;

        internal ConversionPath(params IConversion[] rawElements)
            : this()
        {
            Tracer.Assert(rawElements.Any());
            Source = rawElements.First().Source;
            Tracer.Assert(IsValid);
            Elements = rawElements.RemoveCircles().ToArray();

            AssertValid();
            StopByObjectIds(-284);
        }

        void AssertValid()
        {
            if(Elements.Any())
                Tracer.Assert
                    (
                        Source == Elements.First().Source,
                        () =>
                            "Wrong source type: "
                                + Source
                                + " should be: "
                                + Elements.First().Source);

            Tracer.Assert
                (
                    Types.Count() == Types.Distinct().Count(),
                    () => "Cyclic conversion:\n"
                        + Types.Select(t => t.DumpPrintText).Stringify("\n")
                        + "\n****\n"
                        + Dump()
                );

            var typesByDestination = TypesByDestination.ToArray();

            var merge = Types.Select
                (
                    (item, index) => new
                    {
                        index,
                        type = item,
                        typeDestination = typesByDestination[index]
                    }
                )
                .Where(item => item.type != item.typeDestination)
                .ToArray();

            Tracer.Assert
                (
                    !merge.Any(),
                    () =>
                        "Inconsistent path:\n"
                            + Source.Dump()
                            + "\n"
                            + Tracer.Dump
                                (
                                    Elements.Select
                                        (
                                            item => new
                                            {
                                                item.Source,
                                                Destination = item.ResultType()
                                            }
                                        )
                                        .ToArray()
                                )
                );
        }

        IEnumerable<TypeBase> Types
            => Elements
                .Select(element => element.Source)
                .Concat(new[] {Destination})
                .ToArray();

        IEnumerable<TypeBase> TypesByDestination
            => new[] {Source}
                .Concat(Elements.Select(element => element.ResultType()))
                .ToArray();

        IEnumerable<string> DumpConversions
            => Elements
                .Select(element => element.Result(Category.Code).Code.DebuggerDumpString)
                .ToArray();

        internal TypeBase Destination => Elements.LastOrDefault()?.ResultType() ?? Source;

        public static ConversionPath operator +(ConversionPath a, ConversionPath b)
            => new ConversionPath(a.Elements.Concat(b.Elements).ToArray());

        public static IEnumerable<ConversionPath> operator +
            (
            IEnumerable<ConversionPath> a,
            ConversionPath b
            )
            => a.Select(left => left + b);

        public static IEnumerable<ConversionPath> operator +(
            ConversionPath a,
            IEnumerable<ConversionPath> b)
            => b.Select(right => a + right);

        public static IEnumerable<ConversionPath> operator +(
            ConversionPath a,
            IEnumerable<IConversion> b)
            => b.Select(right => a + right);

        public static ConversionPath operator +(IConversion a, ConversionPath b)
            => new ConversionPath(new[] {a}.Concat(b.Elements).ToArray());

        public static ConversionPath operator +(ConversionPath a, IConversion b)
            => new ConversionPath(a.Elements.Concat(new[] {b}).ToArray());

        internal Result Execute(Category category)
        {
            var results = Elements
                .Select(item => item.Result(category.Typed))
                //  .ToArray()
                ;
            //Tracer.FlaggedLine("\n"+Tracer.Dump(results));
            var result = results
                .Aggregate(new ResultCache(this), (c, n) => n.ReplaceArg(c))
                & category;
            //Tracer.FlaggedLine("\n"+Tracer.Dump(results));
            return result;
        }

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

        ResultCache.IResultProvider ResultCache.IResultProvider.FindSource(IContextReference ext)
        {
            NotImplementedMethod(ext);
            return null;
        }
    }
}