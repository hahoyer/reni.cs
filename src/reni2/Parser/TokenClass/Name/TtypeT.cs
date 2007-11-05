using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    sealed class TtypeT: Defineable
    {
        internal override Type.SearchResultFromRef SearchFromRef(DefineableToken defineableToken, Ref searchingType)
        {
            return new SearchResultFromRef();
        }

        sealed internal class SearchResultFromRef : Type.SearchResultFromRef
        {
            internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args, Ref definingType)
            {
                if (args == null)
                    return definingType.Target.TypeOperator(category);
                Result argResult = args.Visit(callContext, category | Category.Type);
                return definingType.Target.ApplyTypeOperator(argResult);
            }
        }

    }
}
