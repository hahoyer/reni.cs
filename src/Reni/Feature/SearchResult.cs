using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature
{
    sealed class SearchResult : DumpableObject, IImplementation
    {
        static int _nextObjectId;

        public static SearchResult Create(IImplementation feature, TypeBase definingItem)
        {
            var searchResult = feature as SearchResult;
            if(searchResult == null)
                return new SearchResult(feature, definingItem);
            var source = searchResult.Source;
            Tracer.Assert(source == definingItem);
            return searchResult;
        }

        IImplementation Feature { get; }
        [EnableDump]
        ConversionPath ConverterPath { get; }

        internal SearchResult(SearchResult searchResult, ConversionPath relativeConversion)
            : this(searchResult.Feature, relativeConversion + searchResult.ConverterPath)
        {
            Tracer.Assert(searchResult.Source == relativeConversion.Destination);
        }

        SearchResult(IImplementation feature, TypeBase definingItem)
            : this(feature, new ConversionPath(definingItem)) {}

        SearchResult(IImplementation feature, ConversionPath converterPath)
            : base(_nextObjectId++)
        {
            Feature = feature;
            ConverterPath = converterPath;
            StopByObjectIds();
        }

        [DisableDump]
        internal TypeBase Source => ConverterPath.Source;

        internal Result Execute
            (
            Category category,
            ResultCache left,
            IToken currentTarget,
            ContextBase context,
            ValueSyntax right)
        {
            var trace = ObjectId.In(-34) && category.HasCode;
            StartMethodDump(trace, category, left, currentTarget, context, right);
            try
            {
                var metaFeature = ((IMetaImplementation) Feature).Function;
                if(metaFeature != null)
                    return metaFeature.Result(category, left, context, right);

                BreakExecution();
                var result = Feature.Result(category.WithType, currentTarget, context, right);
                Dump(nameof(result), result);
                Dump("ConverterPath.Destination.CheckedReference", ConverterPath.Destination.CheckedReference);
                Dump("ConverterPath.Execute", ConverterPath.Execute(Category.Code));
                BreakExecution();

                var replaceAbsolute = result
                    .ReplaceAbsolute
                    (ConverterPath.Destination.CheckedReference, ConverterPath.Execute);
                Dump(nameof(replaceAbsolute), replaceAbsolute);
                if(trace)
                    Dump(nameof(left), left.Code);
                BreakExecution();

                return ReturnMethodDump(replaceAbsolute.ReplaceArg(left));

            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result SpecialExecute(Category category)
            => Feature.Result(category, null, null, null);

        internal bool HasHigherPriority(SearchResult other)
            => (Feature is AccessFeature) == (other.Feature is AccessFeature)
                ? ConverterPath.HasHigherPriority(other.ConverterPath)
                : Feature is AccessFeature;

        IFunction IEvalImplementation.Function
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        IValue IEvalImplementation.Value
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        IMeta IMetaImplementation.Function
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }
}