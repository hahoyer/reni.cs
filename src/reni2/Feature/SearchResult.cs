using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature
{
    sealed class SearchResult : FeatureContainer
    {
        internal SearchResult(SearchResult result, ConversionPath relativeConversion)
            : base(result.Feature, result.RootContext)
        {
            ConverterPath = result.ConverterPath + relativeConversion;
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
            : base(feature, definingItem.RootContext)
        {
            ConverterPath = new ConversionPath(definingItem);
            StopByObjectId(-37);
        }

        ConversionPath ConverterPath { get; }

        internal Result Execute
            (Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            var trace = ObjectId == -69 && category.HasCode;
            StartMethodDump(trace, category, left.Data, context, right);
            try
            {
                var result1 = ResultForDebug(category.Typed, context, right);
                if(result1 == null)
                    return ReturnMethodDump<Result>(null);

                var result = result1
                    .ReplaceAbsolute
                    (ConverterPath.Destination.CheckedReference, ConverterPath.Execute);

                Dump(nameof(result1), result1);
                Dump(nameof(result), result);
                Dump(nameof(left), ()=>left.Code);
                BreakExecution();
                return ReturnMethodDump(result.ReplaceArg(left));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result SpecialExecute(Category category) => Result(category, null, null);

        internal bool HasHigherPriority(SearchResult other)
            => (Feature is AccessFeature) == (other.Feature is AccessFeature)
                ? ConverterPath.HasHigherPriority(other.ConverterPath)
                : Feature is AccessFeature;
    }
}