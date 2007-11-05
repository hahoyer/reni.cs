using Reni.Code;

namespace Reni.Parser.TokenClass.Name
{
    sealed class T_A_T_T: Defineable
    {
        internal override StructContainerSearchResult SearchFromStruct()
        {
            return new OperationResult();
        }
        internal sealed class OperationResult : StructContainerSearchResult
        {
            internal override Result Visit(Struct.Container definingContainer, Context.Base definingParentContext,
                                           Context.Base callContext, Category category, Syntax.Base args)
            {
                return definingContainer.VisitOperationApply(definingParentContext, callContext, category, args);
            }
        }
    }
}
