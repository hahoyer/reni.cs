using Reni.Type;

namespace Reni.Parser.TokenClass.Symbol
{
    sealed internal class ColonEqual : Defineable
    {
        internal override string Name { get { return ":="; } }
        sealed internal class SearchResultFromRef 
        {
            public SearchResultFromRef(Ref definingType)
            {
            }

            //internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            //{
            //    if (category.HasCode || category.HasRefs)
            //        return DefiningType.AssignmentOperator(args.Visit(callContext, category | Category.Type));
            //    return Type.Base.CreateVoid.CreateResult(category);
            //}
        }
    }
}
