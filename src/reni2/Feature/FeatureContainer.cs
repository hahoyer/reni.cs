using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
            Tracer.Assert(!(IsImplicit && IsValue));
        }

        protected Root RootContext { get; }
        protected IFeatureImplementation Feature { get; }
        bool IsImplicit => Feature.Function != null && Feature.Function.IsImplicit;
        bool IsValue => Feature.Value != null;

        protected Result Result(Category category, ContextBase context, CompileSyntax right)
        {
            var valueCategory = category;
            if(right != null)
                valueCategory = category.Typed;

            var valueResult = IsValue
                ? Feature.Value.Result(valueCategory)
                : IsImplicit
                    ? Feature.Function.Result(valueCategory, RootContext.VoidType)
                    : null;

            if(right == null)
                return valueResult;

            if(valueResult == null)
                return Feature
                    .Function
                    .Result(category, context.ResultAsReferenceCache(right).Type)
                    .ReplaceArg(context.ResultAsReferenceCache(right));

            var searchResult = valueResult.Type.FuncionDeclarationForTypeAndCloseRelatives;
            if(searchResult != null)
                return searchResult.Execute(category, valueResult, context, right);

            NotImplementedMethod(category, context, right, nameof(valueResult), valueResult);
            return null;
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