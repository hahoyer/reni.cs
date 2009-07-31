using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Feature
{
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
        bool IsEval { get; }
        TypeBase ResultType { get; }
        Result Apply(Category category, Result objectResult);
    }

    internal interface IInfixFeature
    {
        bool IsEvalLeft { get; }
        TypeBase ResultType { get; }
        Result Apply(Category category, Result leftResult, Result rightResult);
    }

    internal interface IContextFeature<TFeature>

    {
    }
}