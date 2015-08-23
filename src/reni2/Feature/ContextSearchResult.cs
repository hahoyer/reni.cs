using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.Feature
{
    sealed class ContextSearchResult : FeatureContainer
    {
        internal ContextSearchResult(IFeatureImplementation feature, Root rootContext)
            : base(feature, rootContext) { Tracer.Assert(feature != null); }

        public Result Execute
            (Category category, Func<Category, Result> objectReference, ContextBase context, CompileSyntax right, SourcePart token)
        {
            var metaFeature = Feature.ContextMeta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, right);

            return Result(category, context, right, token)
                .ReplaceArg(objectReference);
        }
    }
}