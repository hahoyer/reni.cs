using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;

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

            var rightResult = new ResultCache(c=> right == null 
                ? RootContext.VoidType.Result(c)
                : context.ResultAsReference(c,right));

            return Feature
                .Function
                .ApplyResult(category, rightResult.Type)
                .ReplaceArg(rightResult);
        }
    }


}