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

    internal interface IStructFeature
    {
        IContextFeature Convert(ContextAtPosition contextAtPosition);
        IFeature Convert(Struct.Type type);
    }

    internal interface IRefToStructFeature
    {
    }

    internal interface ISequenceOfBitPrefixFeature
    {
        ISequenceElementPrefixFeature Convert();
    }

    internal interface ISequenceOfBitFeature
    {
        ISequenceElementFeature Convert();
    }

    internal interface ISequenceElementFeature
    {
        IFeature Convert(Sequence sequence);
    }

    internal interface ISequenceElementPrefixFeature
    {
        IPrefixFeature Convert(Sequence sequence);
    }

    internal interface IFeatureForSequence
    {
        IFeature Convert(Sequence sequence);
    }

    internal interface IRefToSequenceFeature
    {
        IRefFeature Convert(Sequence sequence);
    }

    internal interface IRefFeature
    {
        IFeature Convert(Ref @ref);
    }
}