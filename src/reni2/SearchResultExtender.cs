using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;

namespace Reni
{
    internal static class SearchResultExtender
    {
        internal static TFeature CheckedConvert<TFeature, TType>(this IConverter<TFeature, TType> feature, TType target)
            where TFeature : class
            where TType : IDumpShortProvider
        {
            if(feature == null)
                return null;
            return feature.Convert(target);
        }
    }
}