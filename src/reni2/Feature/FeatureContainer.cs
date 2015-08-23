using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Type;


namespace Reni.Feature
{
    abstract class FeatureContainer : DumpableObject
    {
        protected FeatureContainer(IFeatureImplementation feature, Root rootContext)
        {
            RootContext = rootContext;
            Feature = feature;
            Tracer.Assert(!(IsImplicit && Feature.Value != null));
        }

        protected Root RootContext { get; }
        protected IFeatureImplementation Feature { get; }
        bool IsImplicit => Feature.Function != null && Feature.Function.IsImplicit;

        protected Result Result(Category category, ContextBase context, CompileSyntax right, SourcePart token)
        {
            var valueCategory = category;
            if(right != null)
                valueCategory = category.Typed;

            var valueResult = IsImplicit
                ? Feature
                    .Function
                    .Result(valueCategory, RootContext.VoidType)
                    .ReplaceArg(RootContext.VoidType.Result(Category.All))
                : right == null || Feature.Function == null
                    ? Feature.Value?.Result(valueCategory)
                    : null;

            if(right == null)
            {
                if(valueResult == null)
                    NotImplementedMethod(category, context, right);
                return valueResult;
            }

            if(valueResult == null)
            {
                if(Feature.Function == null)
                    NotImplementedMethod(category, context, right);
                Tracer.Assert(Feature.Function != null);
                return Feature
                    .Function
                    .Result(category, context.ResultAsReferenceCache(right).Type)
                    .ReplaceArg(context.ResultAsReferenceCache(right));
            }

            return valueResult
                .Type.Execute(category,valueResult,token,null,context,right);
        }

        protected Result ResultForDebug(Category category, ContextBase context, CompileSyntax right, SourcePart token)
        {
            var trace = ObjectId == -1032;
            StartMethodDump(trace, category, context, right);
            try
            {
                BreakExecution();
                var result = Result(category, context, right, token);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}