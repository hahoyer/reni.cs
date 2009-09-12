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

    internal interface IFeature 
    {
        TypeBase ResultType { get; }
        Result Apply(Category category, TypeBase objectType);
    }

    internal interface IPrefixFeature 
    {
        IFeature Feature { get; }
    }


    static class Extender
    {
        internal static IFeature Feature(this object feature)
        {
            if (feature is IFeature)
                return (IFeature) feature;
            return ((IPrefixFeature) feature).Feature;
        }
    }
    internal interface IContextFeature
    {
    }
}