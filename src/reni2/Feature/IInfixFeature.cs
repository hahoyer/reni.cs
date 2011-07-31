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
        Result ObtainResult(Category category, RefAlignParam refAlignParam);
        TypeBase ObjectType { get; }
    }

    internal interface IPrefixFeature
    {
        IFeature Feature { get; }
    }


    internal interface IContextFeature
    {
        Result ObtainResult(Category category);
    }

    internal sealed class Feature : IFeature
    {
        [EnableDump]
        private readonly Func<Category, RefAlignParam, Result> _function;

        public Feature(Func<Category, RefAlignParam, Result> function)
        {
            _function = function;
            Tracer.Assert(_function.Target is TypeBase);
        }

        Result IFeature.ObtainResult(Category category, RefAlignParam refAlignParam) { return _function(category, refAlignParam); }
        TypeBase IFeature.ObjectType { get { return (TypeBase) _function.Target; } }
    }

    internal static class FeatureExtender
    {
        internal static TypeBase TypeOfArgInApplyResult(this IFeature feature, RefAlignParam refAlignParam)
        {
            return feature
                .ObjectType
                .ForceReference(refAlignParam);
        }

        internal static IFeature ConvertToFeature(this object feature)
        {
            if (feature is IPrefixFeature)
                return ((IPrefixFeature)feature).Feature;
            return (IFeature)feature;
        }
    }
}