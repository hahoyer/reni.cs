using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
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
        Root RootContext { get; }

        internal SearchResult(SearchResult result, ConversionPath relativeConversion)
        {
            RootContext = result.RootContext;
            Feature = result.Feature;
            ConverterPath = result.ConverterPath + relativeConversion;
            StopByObjectId(-37);
        }

        internal SearchResult(IFeatureImplementation feature, TypeBase definingItem)
        {
            Feature = feature;
            RootContext = definingItem.RootContext;
            ConverterPath = new ConversionPath(definingItem);
            StopByObjectId(-37);
        }

        internal Result ExecuteForDebug
            (Category category, ResultCache left, ContextBase context, CompileSyntax right, CompileSyntax leftSyntax)
        {
            var trace = ObjectId == -124 && category.HasCode;
            if(!trace)
                return Execute(category, left, context, right);

            StartMethodDump(trace, category, left & category, context, right, leftSyntax);
            try
            {
                var r = Result(category.Typed, right == null ? null : context.ResultCache(right));
                Dump("r", r);
                BreakExecution();

                var rr = r
                    .ReplaceAbsolute(ConverterPath.Destination.CheckedReference, ConverterPath.Execute)
                    ;
                Dump("rr", rr);
                BreakExecution();

                var rrr = rr.ReplaceArg(left);
                Dump("rrr", rrr);
                BreakExecution();

                return ReturnMethodDump(Execute(category, left, context, right), false);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result Execute(Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(category, left, context, right);

            return Result(category, left, right == null ? null : new ResultCache(c => context.ResultAsReference(c, right)));
        }

        Result Result(Category category, ResultCache left, ResultCache right) 
            => Result(category.Typed, right)
            .ReplaceAbsolute(ConverterPath.Destination.CheckedReference, ConverterPath.Execute)
            .ReplaceArg(left);

        Result Result(Category category, ResultCache rightArg)
        {
            var simpleFeature = Feature.SimpleFeature();
            if(simpleFeature != null && rightArg == null)
                return simpleFeature.Result(category);

            var right = rightArg ?? RootContext.VoidType.Result(Category.All);

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