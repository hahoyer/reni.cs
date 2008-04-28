using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tdump_printT : Defineable, IFeature
    {
        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this, this);
        }

        public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, args,callObject);

            return callObject.DumpPrint(category);
        }
        internal override string Name { get { return "dump_print"; } }
    }
}