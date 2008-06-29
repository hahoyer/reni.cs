using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    internal interface IFeature
    {
        Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args);
    }

    internal interface IPrefixFeature
    {
        Result Result(Category category, Result argResult);
    }

    internal interface IContextFeature
    {
        Result VisitApply(ContextBase contextBase, Category category, ICompileSyntax args);
    }

    internal interface IStructFeature
    {
        IContextFeature Convert(Struct.Context context);
        IFeature Convert(Struct.Type type);
    }

    internal interface IRefToStructFeature {}

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
