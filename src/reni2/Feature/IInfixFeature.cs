using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
    internal interface IConverter<TOutType, TInType>
    {
        TOutType Convert(TInType type);
    }

    internal interface ISearchPath<TOutType, TInType>
    {
        TOutType Convert(TInType type);
    }

    internal interface ISimpleFeature
    {
        TypeBase ResultType { get; }
        Result Apply(Category category);
    }

    internal interface ISuffixFeature : IUnaryFeature
    {
    }

    internal interface IPrefixFeature : IUnaryFeature

    {
    }

    internal interface IUnaryFeature
    {
        TypeBase ResultType { get; }
        Result Apply(Category category, TypeBase objectType);
    }

    internal interface IInfixFeature
    {
        TypeBase ResultType { get; }
        Result Apply(Category category, TypeBase leftType, TypeBase rightType);
    }

    internal interface IContextFeature<TFeature>

    {
    }
}