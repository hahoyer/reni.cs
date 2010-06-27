// #pragma warning disable 649
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
        Result Apply(Category category);
        TypeBase DefiningType();
    }

    internal interface IPrefixFeature
    {
        IFeature Feature { get; }
    }


    internal static class Extender
    {
        internal static IFeature ConvertToFeature(this object feature)
        {
            if(feature is IPrefixFeature)
                return ((IPrefixFeature) feature).Feature;
            return (IFeature) feature;
        }
    }

    internal interface IContextFeature
    {
        Result Apply(Category category);
    }
}