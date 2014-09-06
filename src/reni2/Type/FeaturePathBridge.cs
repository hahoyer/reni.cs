using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Feature;

namespace Reni.Type
{
    sealed class FeaturePathBridge<TProvider> : DumpableObject, ISearchTarget
        where TProvider : TypeBase
    {
        [EnableDump]
        readonly TProvider _innerProvider;
        [EnableDump]
        readonly TypeBase _mainProvider;

        public FeaturePathBridge(TProvider innerProvider, TypeBase mainProvider)
        {
            _innerProvider = innerProvider;
            _mainProvider = mainProvider;
        }

        ISearchResult ISearchTarget.GetFeature<TDefinable, TPath>()
        {
            var searchResult = _mainProvider
                .GetSearchResult(new Path<TDefinable, IPath<TPath, TProvider>>());
            return searchResult == null ? null : searchResult.Convert(_innerProvider);
        }
    }
}