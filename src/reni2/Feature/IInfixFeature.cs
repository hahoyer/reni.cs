using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Type;

namespace Reni.Feature
{
    internal interface ISearchPath<TOutType, TInType>
    {
        TOutType Convert(TInType type);
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

    internal interface IContextFeature
    {
    }
}