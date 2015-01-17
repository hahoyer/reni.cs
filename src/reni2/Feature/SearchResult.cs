using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.Type;
using Reni.Type.ConversionService;

namespace Reni.Feature
{
    sealed class SearchResult : DumpableObject
    {
        public IFeatureImplementation Feature { get; }
        public Path ConverterPath { get; }

        internal SearchResult(SearchResult result, Path relativeConversion)
        {
            ConverterPath = result.ConverterPath + relativeConversion;
            Feature = result.Feature;
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
        {
            Feature = feature;
            ConverterPath = new Path(definingItem.Pointer);
        }
        CallDescriptor CallDescriptor => new CallDescriptor(Feature, ConverterPath);

        internal Result CallResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => CallDescriptor.Result(category, context, left, right, ConverterPath.Execute);

        internal Result CallResult(Category category) => CallDescriptor.Result(category, ConverterPath.Execute);
    }
}