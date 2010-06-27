using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Void : TypeBase
    {
        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected override TypeBase CreateReversePair(TypeBase first) { return first; }
        protected override Size GetSize() { return Size.Zero; }

        internal override TypeBase CreatePair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        internal override string DumpShort() { return "void"; }
        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature) { return false; }
        internal override bool IsVoid { get { return true; } }
    }
}