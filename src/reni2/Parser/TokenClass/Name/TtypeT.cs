using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    sealed class TtypeT: Defineable
    {
        sealed internal class SearchResultFromRef
        {
            private readonly Type.Base _definingTargetType;

            public SearchResultFromRef(Ref definingType) 
            {
                _definingTargetType = definingType.Target;
            }

            //internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            //{
            //    if (args == null)
            //        return _definingTargetType.TypeOperator(category);
            //    Result argResult = args.Visit(callContext, category | Category.Type);
            //    return _definingTargetType.ApplyTypeOperator(argResult);
            //}
        }

    }
}
