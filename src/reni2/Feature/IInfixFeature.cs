using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Type;

namespace Reni.Feature
{
    internal interface ISearchPath<out TOutType, in TInType>
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

    internal class Feature : IFeature
    {
        private readonly Func<Category, Result> _func;

        public Feature(Func<Category, Result> func)
        {
            _func = func;
            Tracer.Assert(_func.Target is TypeBase);
        }

        Result IFeature.Apply(Category category) { return _func(category); }
        TypeBase IFeature.DefiningType() { return (TypeBase) _func.Target; }
    }
}