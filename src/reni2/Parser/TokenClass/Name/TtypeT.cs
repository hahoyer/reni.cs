using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TtypeT : Defineable, IFeature
    {
        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref objectType)
        {
            if(args == null)
                return objectType.TypeOperator(category);
            var argResult = args.Visit(callContext, category | Category.Type);
            return objectType.ApplyTypeOperator(argResult);
        }

        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this, this);
        }
    }
}