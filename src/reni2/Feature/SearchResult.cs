using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
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
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
        {
            Feature = feature;
            ConverterPath = new ConversionPath(definingItem);
        }
        
        internal Result Execute
            (
            Category category,
            ResultCache left,
            ContextBase context,
            CompileSyntax right,
            SourcePart token)
        {
            var metaFeature = ((IMetaFeatureImplementation) Feature).Function;
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