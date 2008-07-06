using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{

    internal interface IConverter<OutType, InType>
    {
        OutType Convert(InType inObject);
    }

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
        Result ApplyResult(ContextBase contextBase, Category category, ICompileSyntax args);
    }

    internal interface IRefToStructFeature {}
    internal interface IFeatureForSequence: IConverter<IFeature, Sequence>{}
    internal interface IRefToSequenceFeature: IConverter<IRefFeature, Sequence>{}
    internal interface IRefFeature: IConverter<IFeature, AssignableRef >{}
}
