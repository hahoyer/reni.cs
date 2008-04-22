using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tenable_cutT : Defineable
    {
        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref objectType,
            TypeBase definingType)
        {
            NotImplementedMethod(callContext, category, args, objectType, definingType);
            return null;
        }
    }
}