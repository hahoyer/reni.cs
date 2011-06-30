using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;

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

        internal override TypeBase ToReference(RefAlignParam refAlignParam) { return this; }

        protected override TypeBase ReversePair(TypeBase first) { return first; }
        protected override Size GetSize() { return Size.Zero; }

        internal override TypeBase Pair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        internal override string DumpShort() { return "void"; }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return false; }
        [DisableDump]
        internal override bool IsVoid { get { return true; } }
    }
}