using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Feature
{
    internal interface IConverter<TOutType, TInType>
    {
        TOutType Convert(TInType type);
    }

    internal interface IFeature
    {
        Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args);
    }

    internal interface IPrefixFeature
    {
        Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object);
    }

    internal interface IContextFeature
    {
        Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax args);
    }
}