using HWClassLibrary.Debug;
using Reni.Code;
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

        public Result Result(ContextBase callContext, Category category, ICompileSyntax args, Ref callObject)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, args, callObject);

            return callObject.DumpPrint(category);
        }

        internal override string Name { get { return "dump_print"; } }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);

            return callContext.ApplyResult(category, @object, ot => ot.DumpPrint(category));
        }
    }
}