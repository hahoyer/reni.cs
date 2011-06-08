using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;

namespace Reni
{
    internal static class SearchResultExtender
    {
        internal static TFeature CheckedConvert<TFeature, TType>(this ISearchPath<TFeature, TType> feature, TType target)
            where TFeature : class
            where TType : IDumpShortProvider
        {
            if(feature == null)
                return null;
            return feature.Convert(target);
        }
    }
}