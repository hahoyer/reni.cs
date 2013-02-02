using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    sealed class SearchResultWithConversion : SearchResult 
    {
        readonly SearchResult _parent;
        readonly IProxyType _proxyType;
        
        internal SearchResultWithConversion(SearchResult parent, IProxyType proxyType)
        {
            _parent = parent;
            _proxyType = proxyType;
        }

        [DisableDump]
        internal override TypeBase DefiningType { get { return _parent.DefiningType; } }
        [DisableDump]
        internal override IFeature Feature { get { return _parent.Feature; } }

        protected override Result ConverterResult(Category category)
        {
            var result = base.ConverterResult(category);
            var converterResult = _proxyType.Converter.Result(category);
            return result == null ? converterResult : result.ReplaceArg(converterResult);
        }
    }
}