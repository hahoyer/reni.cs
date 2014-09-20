using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    sealed class AlignConverter : ConverterBase
    {
        [EnableDump]
        readonly SearchResult _childConverter;
        public AlignConverter(SearchResult childConverter) { _childConverter = childConverter; }
        protected override Result Result(Category category) { return _childConverter.CallResult(category.Typed); }
    }
}