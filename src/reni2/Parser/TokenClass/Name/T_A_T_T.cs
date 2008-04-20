using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class T_A_T_T : Defineable
    {
        Result Visit(Struct.Type definingType, ContextBase callContext, Category category,
            SyntaxBase args)
        {
            return definingType.Container.VisitOperationApply(definingType.Context, callContext, category, args);
        }
    }
}