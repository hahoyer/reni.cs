using Reni.Code;

namespace Reni.Parser.TokenClass.Name
{
    sealed class T_A_T_T: Defineable
    {
        internal sealed class OperationResult : StructContainerSearchResult
        {
            internal override Result Visit(Struct.Type definingType, Context.Base callContext, Category category,
                                           Syntax.Base args)
            {
                return definingType.Container.VisitOperationApply(definingType.Context, callContext, category, args);
            }
        }
    }
}
