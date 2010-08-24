using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        [IsDumpEnabled(true)]
        private readonly Reference _value;

        private readonly IFunctionalFeature _functionalFeature;

        public TypeType(Reference value)
        {
            _functionalFeature = new ConversionFeature(value.AutomaticDereference());
            _value = value;
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }
        protected internal override Result ConvertToItself(Category category) { return VoidResult(category); }
        internal override IFunctionalFeature FunctionalFeature() { return _functionalFeature; }
        internal override string DumpShort() { return "(" + _value.DumpShort() + ") type"; }
        internal override TypeBase ObjectType() { return _value; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result DumpPrintResult(Category category)
        {
            return Void.Result(category, () => CodeBase.DumpPrintText(_value.DumpPrintText));
        }
    }
}