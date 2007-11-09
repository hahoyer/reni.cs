using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    sealed class TtypeT: Defineable
    {
        internal override Type.SearchResultFromRef SearchFromRef(DefineableToken defineableToken, Ref searchingType)
        {
            return new SearchResultFromRef(searchingType.Target);
        }

        sealed internal class SearchResultFromRef : Type.SearchResultFromRef
        {
            private readonly Type.Base _definingTargetType;

            public SearchResultFromRef(Type.Base definingTargetType)
            {
                _definingTargetType = definingTargetType;
            }

            internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            {
                if (args == null)
                    return _definingTargetType.TypeOperator(category);
                Result argResult = args.Visit(callContext, category | Category.Type);
                return _definingTargetType.ApplyTypeOperator(argResult);
            }
        }

    }
}
