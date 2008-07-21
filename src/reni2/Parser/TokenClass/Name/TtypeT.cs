using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    [Token("type")]
    internal sealed class TtypeT : Defineable, IFeature
    {
        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this, this);
        }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var objectType = callContext.Type(@object);
            if(args == null)
                return objectType.TypeOperator(category);
            return callContext.ApplyResult(category, args, argsType => argsType.Conversion(category, objectType));
        }
    }
}