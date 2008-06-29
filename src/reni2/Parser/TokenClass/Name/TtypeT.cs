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

        public Result Result(ContextBase callContext, Category category, ICompileSyntax args, Ref callObject)
        {
            if (args == null)
                return callObject.TypeOperator(category);
            var argResult = callContext.Result(category | Category.Type, args);
            return callObject.ApplyTypeOperator(argResult);
        }
        internal override string Name { get { return "type"; } }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var objectType = callContext.Type(@object);
            var argsType = callContext.Type(args);

            var argResult = callContext.Result(category | Category.Type, args);


        }
    }
}