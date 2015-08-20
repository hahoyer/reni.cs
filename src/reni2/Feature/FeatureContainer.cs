using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

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

        protected Result Result(Category category, ContextBase context, CompileSyntax right)
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

            var searchResults = valueResult
                .Type
                .DeclarationsForTypeAndCloseRelatives(null)
                .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    NotImplementedMethod(category, context, right, nameof(valueResult), valueResult);
                    return null;
                case 1:
                    return searchResults.Single().Execute(category, valueResult, context, right);
                default:
                    NotImplementedMethod
                        (
                            category,
                            context,
                            right,
                            nameof(valueResult),
                            valueResult,
                            nameof(searchResults),
                            searchResults
                        );
                    return null;
            }
        }

        protected Result ResultForDebug(Category category, ContextBase context, CompileSyntax right)
        {
            var trace = ObjectId == -1032;
            StartMethodDump(trace, category, context, right);
            try
            {
                BreakExecution();
                var result = Result(category, context, right);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}