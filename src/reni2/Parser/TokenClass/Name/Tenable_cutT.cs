using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tenable_cutT : Defineable, IFeature
    {
        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref objectType)
        {
            if(args == null)
                return objectType.CreateEnableCut().CreateArgResult(category);
            NotImplementedMethod(callContext, category, args);
            return null;
        }

        Result Visit(Struct.Type definingType, ContextBase callContext, Category category,SyntaxBase args)
        {
            if(args == null)
                return definingType.CreateArgResult(category);
            NotImplementedMethod(callContext, category, args);
            return null;
        }

        internal override SearchResult<IFeature> SearchFromSequence()
        {
            return SearchResult<IFeature>.Success(this,this);
        }
    }
}