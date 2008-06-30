using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class TtypeT : Defineable, IFeature
    {
        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this,this);
        }

        internal override string Name { get { return "type"; } }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var objectType = callContext.Type(@object);
            return callContext.ApplyResult(category, args, argsType => argsType.Conversion(category, objectType));
        }
    }
}