using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual: Defineable
    {
        internal override SearchResultFromRef SearchResultFromRef
        {
            get { return new AssignmentSearchResult(); }
        }
    }

    sealed internal class AssignmentSearchResult : SearchResultFromRef
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
