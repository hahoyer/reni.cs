using System;
using System.Collections.Generic;
using System.Linq;
using Reni.TokenClasses;

namespace Reni.Feature.DumpPrint
{
    sealed class DumpPrintToken : Defineable
    {
        internal static DumpPrintToken Create() { return new DumpPrintToken {Name = "<dump_print>"}; }
        internal override ISearchResult GetFeatureGenericized(ISearchTarget target) { return target.GetFeature(this); }
        internal override SearchResult Declarations<TType>(TType target) { return target.DeclarationsForType<DumpPrintToken>(); }
    }
}