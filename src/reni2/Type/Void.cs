using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    internal sealed class Void : TypeBase
    {
        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

        protected override TypeBase CreateReversePair(TypeBase first) { return first; }
        protected override Size GetSize() { return Size.Zero; }

        internal override TypeBase CreatePair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        internal override string DumpShort() { return "void"; }
        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature) { return false; }
        internal override bool IsVoid { get { return true; } }

        internal new static Result CreateResult(Category category) { return CreateVoid.CreateResult(category); }
        internal new static Result CreateResult(Category category, Func<CodeBase> getCode) { return CreateVoid.CreateResult(category, getCode); }
    }
}