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
    sealed class SearchResult : DumpableObject, IImplementation
    {
        static int _nextObjectId;

        public static SearchResult Create(IImplementation feature, TypeBase definingItem)
        {
            var searchResult = feature as SearchResult;
            if(searchResult == null)
                return new SearchResult(feature, definingItem);
            Tracer.Assert(searchResult.Source == definingItem);
            return searchResult;
        }

        IImplementation Feature { get; }
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
            ContextBase context,
            CompileSyntax right,
            SourcePart token)
        {
            var metaFeature = ((IMetaImplementation) Feature).Function;
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

        IFunction IEvalImplementation.Function
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        IConversion IEvalImplementation.Conversion
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