using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature
{
    abstract class FeatureContainer : DumpableObject
    {
        protected FeatureContainer(IFeatureImplementation feature, Root rootContext)
        {
            RootContext = rootContext;
            Feature = feature;
        }

        protected Root RootContext { get; }
        protected IFeatureImplementation Feature { get; }

        protected Result Result(Category category, ContextBase context, CompileSyntax right)
        {
            var simpleFeature = Feature.SimpleFeature();
            if(simpleFeature != null && right == null)
                return simpleFeature.Result(category);

            var rightResult = new ResultCache
                (
                c => right == null
                    ? RootContext.VoidType.Result(c)
                    : context.ResultAsReference(c, right));

            return Feature
                .Function
                .ApplyResult(category, rightResult.Type)
                .ReplaceArg(rightResult);
        }

        protected Result ResultForDebug(Category category, ContextBase context, CompileSyntax right)
        {
            var trace = ObjectId == -1032;
            StartMethodDump(trace, category, context, right);
            try
            {
                BreakExecution();
                var result = Result(category,context,right);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }

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

        internal Result Execute(Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.Meta;
            if (metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            var trace = ObjectId == -69 && category.HasCode;
            StartMethodDump(trace, category, left.Data, context, right);
            try
            {
                var result1 = ResultForDebug(category.Typed, context, right);
                var result = result1
                    .ReplaceAbsolute(ConverterPath.Destination.CheckedReference, ConverterPath.Execute);

                Dump(nameof(result1), result1);
                Dump(nameof(result), result);
                Dump(nameof(left), left.Code);
                BreakExecution();
                return ReturnMethodDump(result.ReplaceArg(left));
            }
            finally
            {
                EndMethodDump();

            }
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
            : base(feature, rootContext)
        {
            Tracer.Assert(feature != null);
        }

        public Result Execute(Category category, Func<Category, Result> objectReference, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.ContextMeta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, right);

            return Result(category, context, right)
                .ReplaceArg(objectReference);
        }
    }
}