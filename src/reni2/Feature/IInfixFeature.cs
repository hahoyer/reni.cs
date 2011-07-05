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
        TypeBase ObjectType { get; }
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
        private readonly Func<Category, Result> _function;

        public Feature(Func<Category, Result> function)
        {
            _function = function;
            Tracer.Assert(_function.Target is TypeBase);
        }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return _function(category); }
        TypeBase IFeature.ObjectType { get { return (TypeBase) _function.Target; } }
    }

    internal static class FeatureExtender
    {
        internal static TypeBase TypeOfArgInApplyResult(this IFeature feature, RefAlignParam refAlignParam)
        {
            return feature
                .ObjectType
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