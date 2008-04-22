using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tdump_printT : Defineable
    {
        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args)
        {

            NotImplementedMethod(callContext, category, args);
            return null;

            //return definingType.DumpPrintFromRef(category, objectType.RefAlignParam).UseWithArg(objectType.CreateArgResult(category));
        }

    }
}