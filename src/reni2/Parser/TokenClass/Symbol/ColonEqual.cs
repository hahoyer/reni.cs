using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual : Defineable
    {
        internal override Type.SearchResultFromRef SearchFromRef(DefineableToken defineableToken, Ref searchingType)
        {
            return new SearchResultFromRef();
        }

        sealed internal class SearchResultFromRef : Type.SearchResultFromRef
        {
            internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args,
                                                Ref definingType)
            {
                if (category.HasCode || category.HasRefs)
                    return definingType.AssignmentOperator(args.Visit(callContext, category | Category.Type));
                return Type.Base.CreateVoid.CreateResult(category);
            }
        }
    }
}
