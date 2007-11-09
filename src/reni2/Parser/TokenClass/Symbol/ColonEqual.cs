using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual : Defineable
    {
        internal override Type.SearchResultFromRef SearchFromRef(DefineableToken defineableToken, Ref searchingType)
        {
            return new SearchResultFromRef(searchingType);
        }

        sealed internal class SearchResultFromRef : Type.SearchResultFromRef
        {
            private readonly Ref _definingType;

            public SearchResultFromRef(Ref definingType)
            {
                _definingType = definingType;
            }

            internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            {
                if (category.HasCode || category.HasRefs)
                    return _definingType.AssignmentOperator(args.Visit(callContext, category | Category.Type));
                return Type.Base.CreateVoid.CreateResult(category);
            }
        }
    }
}
