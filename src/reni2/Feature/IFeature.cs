using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{

    internal interface IConverter<OutType, InType>
    {
        OutType Convert(InType type);
    }

    internal interface IFeature
    {
        Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args);
    }

    internal interface IPrefixFeature
    {
        Result ApplyResult(ContextBase contextBase, Category category, ICompileSyntax @object);
    }

    internal interface IContextFeature
    {
        Result ApplyResult(ContextBase contextBase, Category category, ICompileSyntax args);
    }

    internal interface IFeatureForSequence: IConverter<IFeature, Sequence>{}
}
