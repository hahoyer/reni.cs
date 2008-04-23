using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TtypeT : Defineable, IFeature
    {
        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this,this);
        }

        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject)
        {
            if (args == null)
                return callObject.TypeOperator(category);
            var argResult = args.Visit(callContext, category | Category.Type);
            return callObject.ApplyTypeOperator(argResult);
        }
    }
}