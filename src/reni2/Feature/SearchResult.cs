using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature
{
    sealed class SearchResult : DumpableObject
    {
        IFeatureImplementation Feature { get; }
        ConversionPath ConverterPath { get; }

        internal SearchResult(SearchResult result, ConversionPath relativeConversion)
        {
            Feature = result.Feature;
            ConverterPath = result.ConverterPath + relativeConversion;
            StopByObjectId(-37);
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
        {
            Feature = feature;
            ConverterPath = new ConversionPath(definingItem);
            StopByObjectId(-37);
        }

        IContextReference ObjectReference => ConverterPath.Destination.CheckedReference.AssertNotNull();

        internal Result Execute(Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            var re = right == null ? null : context.ResultCache(right);
            return Result(category, left, re);
        }

        Result Result(Category category, ResultCache left, ResultCache right)
            => Result(category, right)
                .ReplaceAbsolute(ObjectReference, ConverterPath.Execute)
                .ReplaceArg(left);

        Result Result(Category category, ResultCache right)
        {
            var simpleFeature = Feature.SimpleFeature();
            if(simpleFeature != null && right == null)
            {
                var simpleResult = simpleFeature.Result(category);
                return (simpleResult);
            }

            right = right ?? ConverterPath.Destination.RootContext.VoidType.Result(Category.All);

            return Feature
                .Function
                .ApplyResult(category, right.Type)
                .ReplaceArg(right);
        }

        internal Result CallResult(Category category) => Result(category, null);

        internal bool HasHigherPriority(SearchResult other)
            => (Feature is AccessFeature) == (other.Feature is AccessFeature)
                ? ConverterPath.HasHigherPriority(other.ConverterPath)
                : Feature is AccessFeature;
    }
}