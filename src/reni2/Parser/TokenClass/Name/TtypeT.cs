using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TtypeT : Defineable
    {
        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref objectType, TypeBase definingType)
        {
            NotImplementedMethod(callContext, category, args, objectType, definingType);
            if (args == null)
                return objectType.TypeOperator(category);
            var argResult = args.Visit(callContext, category | Category.Type);
            return objectType.ApplyTypeOperator(argResult);
        }
    }
}