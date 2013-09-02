using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;

namespace Reni.Type
{
    sealed class AlignConverter : ConverterBase
    {
        [EnableDump]
        readonly ISearchResult _childConverter;
        public AlignConverter(ISearchResult childConverter) { _childConverter = childConverter; }
        protected override Result Result(Category category) { return _childConverter.SimpleResult(category.Typed); }
    }
}