using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature
{
    sealed class SearchResult : FeatureContainer
    {
        internal SearchResult(SearchResult result, ConversionPath relativeConversion)
            : base(result.Feature, result.RootContext) { ConverterPath = result.ConverterPath + relativeConversion; }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
            : base(feature, definingItem.RootContext)
        {
            ConverterPath = new ConversionPath(definingItem);
            StopByObjectId(-37);
        }

        ConversionPath ConverterPath { get; }

        internal Result Execute(Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            return Result(category.Typed, context, right)
                .ReplaceAbsolute(ConverterPath.Destination.CheckedReference, ConverterPath.Execute)
                .ReplaceArg(left);
        }

        internal Result CallResult(Category category) => Result(category, null, null);

        internal bool HasHigherPriority(SearchResult other)
            => (Feature is AccessFeature) == (other.Feature is AccessFeature)
                ? ConverterPath.HasHigherPriority(other.ConverterPath)
                : Feature is AccessFeature;
    }

    sealed class ContextSearchResult : FeatureContainer
    {
        internal ContextSearchResult(IFeatureImplementation feature, Root rootContext)
            : base(feature, rootContext) { }

        public Result Execute(Category category, Func<Category, Result> objectReference, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.ContextMeta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, right);

            return Result(category,context,right)
                .ReplaceArg(objectReference);
        }
    }
}