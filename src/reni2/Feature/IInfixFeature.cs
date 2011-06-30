using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Type;

namespace Reni.Feature
{
    internal interface ISearchPath<out TOutType, in TInType>
    {
        TOutType Convert(TInType type);
    }

    internal interface IFeature
    {
        Result Apply(Category category, RefAlignParam refAlignParam);
        TypeBase DefiningType();
    }

    internal interface IPrefixFeature
    {
        IFeature Feature { get; }
    }


    internal interface IContextFeature
    {
        Result Apply(Category category);
    }

    internal sealed class Feature : IFeature
    {
        [EnableDump]
        private readonly Func<Category, Result> _func;

        public Feature(Func<Category, Result> func)
        {
            _func = func;
            Tracer.Assert(_func.Target is TypeBase);
        }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return _func(category); }
        TypeBase IFeature.DefiningType() { return (TypeBase) _func.Target; }
    }

    internal static class FeatureExtender
    {
        internal static TypeBase TypeOfArgInApplyResult(this IFeature feature, RefAlignParam refAlignParam)
        {
            return feature
                .DefiningType()
                .ToReference(refAlignParam);
        }

        internal static IFeature ConvertToFeature(this object feature)
        {
            if (feature is IPrefixFeature)
                return ((IPrefixFeature)feature).Feature;
            return (IFeature)feature;
        }
    }
}