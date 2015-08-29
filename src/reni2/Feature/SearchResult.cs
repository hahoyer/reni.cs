using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
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
            : base(result.Feature)
        {
            ConverterPath = result.ConverterPath + relativeConversion;
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
            : base(feature)
        {
            ConverterPath = new ConversionPath(definingItem);
            StopByObjectId(-37);
        }

        ConversionPath ConverterPath { get; }

        internal Result Execute
            (
            Category category,
            ResultCache left,
            ContextBase context,
            CompileSyntax right,
            SourcePart token)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            return Feature
                .Result(category.Typed, token, context, right)
                .ReplaceAbsolute(ConverterPath.Destination.CheckedReference, ConverterPath.Execute)
                .ReplaceArg(left);
        }

        internal Result SpecialExecute(Category category)
            => Feature.Result(category, null, null, null);

        internal bool HasHigherPriority(SearchResult other)
            => (Feature is AccessFeature) == (other.Feature is AccessFeature)
                ? ConverterPath.HasHigherPriority(other.ConverterPath)
                : Feature is AccessFeature;
    }
}