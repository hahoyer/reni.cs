using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tdump_printT : Defineable, IFeature
    {
        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref objectType)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, args, objectType);

            return objectType.Target.DumpPrintFromRef(category, objectType.RefAlignParam);
        }

        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this, this);
        }
    }
}