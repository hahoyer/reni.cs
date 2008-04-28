using Reni.Context;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    internal interface IFeature
    {
        Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject);
    }

    internal interface IPrefixFeature
    {
        Result VisitApply(Category category, Result argResult);
    }

    internal interface IContextFeature
    {
        Result VisitApply(ContextBase contextBase, Category category, SyntaxBase args);
    }

    internal interface IStructContainerFeature {
        IContextFeature Convert(ContextAtPosition contextAtPosition);
    }

    internal interface ISequenceOfBitPrefixFeature {
        ISequenceElementPrefixFeature Convert();
    }

    internal interface ISequenceOfBitFeature {
        ISequenceElementFeature Convert();
    }

    internal interface ISequenceElementFeature {
        IFeature Convert(Sequence sequence);
    }

    internal interface ISequenceElementPrefixFeature {
        IPrefixFeature Convert(Sequence sequence);
    }

    internal interface ISequenceFeature
    {
        IFeature Convert(Sequence sequence);
    }

    internal interface IRefToSequenceFeature
    {
        IRefFeature Convert(Sequence sequence);
    }

    internal interface IRefFeature {
        IFeature Convert(Ref @ref);
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