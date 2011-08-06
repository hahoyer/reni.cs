using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        [EnableDump]
        private readonly TypeBase _value;

        private readonly IFunctionalFeature _functionalFeature;

        public TypeType(TypeBase value)
        {
            _functionalFeature = new ConversionFeature(value.AutomaticDereference());
            _value = value;
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }

        [DisableDump]
        internal override IFunctionalFeature FunctionalFeature { get { return _functionalFeature; } }

        internal override string DumpShort() { return "(" + _value.DumpShort() + ") type"; }
        [DisableDump]
        internal override TypeBase ObjectType { get { return _value; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam) { return Void.Result(category, () => CodeBase.DumpPrintText(_value.DumpPrintText)); }
    }
}