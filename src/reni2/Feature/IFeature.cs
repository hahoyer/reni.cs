using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    internal interface IFeature
    {
        Result VisitApply(ContextBase context, Category category, SyntaxBase args, Ref objectType);
    }

    internal interface IPrefixFeature
    {
        Result VisitApply(Category category, Result argResult);
    }

    internal interface IContextFeature
    {
        Result VisitApply(ContextBase contextBase, Category category, SyntaxBase args);
    }

    internal class DefaultSearchResultFromRef
    {
        private readonly IFeature _feature;

        public DefaultSearchResultFromRef(IFeature feature, Ref definingType)
        {
            _feature = feature;
        }

        //internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        //{
        //    Result result = _featureBase.VisitApply(callContext, category, args);
        //    result = result.UseWithArg(DefiningType.CreateDereferencedArgResult(category));
        //    return result;
        //}
    }
}