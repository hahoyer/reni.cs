using System.Linq;
using System.Collections.Generic;
using System;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
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
        protected abstract IContextReference ObjectReference { get; }
        [DisableDump]
        protected abstract IFeatureImplementation Feature { get; }

        internal Result Result(Category category, ContextBase context, CompileSyntax left, CompileSyntax right, Func<Category, Result> converterResult)
        {
            var metaFeature = Feature.Meta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, left, right);

            var trace = ObjectId == -62 && category.HasCode;
            StartMethodDump(trace, category, context, left, right,null);
            try
            {
                BreakExecution();
                var rawResult = Result(category, context, right);
                Dump("rawResult", rawResult);
                BreakExecution();

                if(trace)
                {
                    var objectResult = context.ResultAsReference(category, left);
                    Dump("objectResult", objectResult);
                    BreakExecution();

                    var converterResultForAll
                        = converterResult(category.Typed)
                        .ReplaceArg(objectResult)
                        ;
                    Dump("converterResult", converterResultForAll);
                    BreakExecution();

                    Tracer.Assert(converterResultForAll.Type.ForcedReference == ObjectReference);
                }


                var result = rawResult
                    .ReplaceAbsolute(ObjectReference, c=> converterResult(c).ReplaceArg(c1=>context.ResultAsReference(c1,left)));
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
            var trace = ObjectId.In(662) && category.HasCode;
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

                var result = applyResult.ReplaceArg(c => context.ArgsResult(c, right));
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