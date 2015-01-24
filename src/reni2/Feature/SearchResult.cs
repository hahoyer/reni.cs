using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Type;

namespace Reni.Feature
{
    sealed class SearchResult : DumpableObject
    {
        public IFeatureImplementation Feature { get; }
        public ConversionService.Path ConverterPath { get; }

        internal SearchResult(SearchResult result, ConversionService.Path relativeConversion)
        {
            Feature = result.Feature;
            ConverterPath = result.ConverterPath + relativeConversion;
            StopByObjectId(-37);
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
        {
            Feature = feature;
            ConverterPath = new ConversionService.Path(definingItem);
            StopByObjectId(-37);
        }

        CallDescriptor CallDescriptor => new CallDescriptor(Feature, ConverterPath);

        internal Result CallResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => CallDescriptor.Result(category, context, left, right, ConverterPath.Execute);

        internal Result CallResult(Category category) => CallDescriptor.Result(category, ConverterPath.Execute);
    }
}