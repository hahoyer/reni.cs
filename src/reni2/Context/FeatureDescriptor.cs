using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Feature;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.Type;

namespace Reni.Context
{
    abstract class FeatureDescriptor : DumpableObject
    {
        [DisableDump]
        protected abstract TypeBase Type { get; }
        [DisableDump]
        protected abstract IFeatureImplementation Feature { get; }

        internal Result Result(Category category, ContextBase context, CompileSyntax left, CompileSyntax right, Func<Category, Result> converterResult)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, left, right);

            var trace = ObjectId == 92 && category.HasCode;
            StartMethodDump(trace, category, context, left, right,null);
            try
            {
                BreakExecution();
                var rawResult = Result(category, context, right);
                Dump("rawResult", rawResult);
                BreakExecution();

                var converterResultForAll = converterResult(category.Typed);
                Dump("converterResult", converterResultForAll);
                BreakExecution();

                var rawResult2 = rawResult;
                var compoundType = Type.AutomaticDereferenceType as CompoundType;
                if(compoundType != null)
                {
                    rawResult2 = rawResult.ContextViaObjectPointer(compoundType.View);
                    Dump("rawResult2", rawResult2);
                    BreakExecution();
                }

                var resultWithConversions = rawResult2
                    .ReplaceArg(converterResult);
                Dump("resultWithConversions", resultWithConversions);
                BreakExecution();

                var objectResult = context.ObjectResult(category, left);
                Dump("objectResult", objectResult);
                BreakExecution();

                var result = resultWithConversions.ReplaceArg(objectResult);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result Result(Category category, ContextBase context, CompileSyntax right, Func<Category, Result> converterResult)
        {
            var metaFeature = Feature.ContextMeta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, right);

            var result = Result(category, context, right);
            return result.ReplaceArg(converterResult);
        }

        internal Result Result(Category category, Func<Category, Result> converterResult) => SimpleResult(category).ReplaceArg(converterResult);

        Result Result(Category category, ContextBase context, CompileSyntax right)
        {
            var trace = ObjectId.In(-92) && category.HasCode;
            StartMethodDump(trace, category, context, right);
            try
            {
                var simpleFeature = Feature.SimpleFeature();
                if (simpleFeature != null && right == null)
                {
                    var simpleResult = simpleFeature.Result(category);
                    return ReturnMethodDump(simpleResult);
                }

                var function = Feature.Function;
                BreakExecution();

                var argsType = context.ArgsResult(Category.Type, right).Type;
                Dump("argsType", argsType);
                BreakExecution();

                var applyResult = function.ApplyResult(category, argsType);
                Dump("applyResult", applyResult);
                BreakExecution();

                var replaceArg = applyResult.ReplaceArg(c => context.ArgsResult(c, right));
                Dump("replaceArg", replaceArg);
                BreakExecution();

                var result = replaceArg
                    .ReplaceAbsolute(function.ObjectReference, c => Type.Pointer.ArgResult(c.Typed));

                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result SimpleResult(Category category)
        {
            Tracer.Assert(Feature.Meta == null);

            var simpleFeature = Feature.SimpleFeature();
            if(simpleFeature != null)
                return simpleFeature.Result(category);

            var function = Feature.Function;
            var result = function
                .ApplyResult(category, Type.RootContext.VoidType)
                .ReplaceAbsolute(function.ObjectReference, Type.SmartPointer.ArgResult);
            return result;
        }
    }

}