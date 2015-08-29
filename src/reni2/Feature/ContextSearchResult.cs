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
        internal ContextSearchResult(IFeatureImplementation feature)
            : base(feature) { Tracer.Assert(feature != null); }

        internal Result Execute
            (Category category,
            Func<Category, Result> objectReference,
            SourcePart token,
            ContextBase context,
            CompileSyntax right)
        {
            var metaFeature = Feature.ContextMeta;
            if(metaFeature != null)
                return metaFeature.Result(context, category, right);

            return Feature
                .Result(category, token, context, right)
                .ReplaceArg(objectReference);
        }
    }
}