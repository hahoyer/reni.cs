using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    [Serializable]
    internal sealed class TypeType : TypeBase
    {
        [DumpData(true)]
        private readonly TypeBase _value;

        private readonly IFunctionalFeature _functionalFeature;

        private class FunctionalFeature : IFunctionalFeature
        {
            private readonly TypeBase _typeBase;
            public FunctionalFeature(TypeBase typeBase) { _typeBase = typeBase; }
            string IDumpShortProvider.DumpShort() { return _typeBase.DumpShort() + " type"; }

            Result IFunctionalFeature.ContextOperatorFeatureApply(Category category) { throw new NotImplementedException(); }
            Result IFunctionalFeature.DumpPrintFeatureApply(Category category) { throw new NotImplementedException(); }

            Result IFunctionalFeature.Apply(Category category, TypeBase argsType, RefAlignParam refAlignParam)
            {
                return argsType
                    .ConvertTo(category, _typeBase.AutomaticDereference());
            }
        }

        public TypeType(TypeBase value)
        {
            _functionalFeature = new FunctionalFeature(value);
            _value = value;
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }
        protected internal override Result ConvertToItself(Category category) { return CreateVoidResult(category); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _functionalFeature; }
        internal override TypeBase StripFunctional() { return _value; }
        internal override string DumpShort() { return "(" + _value.DumpShort() + ") type"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result CreateDumpPrintResult(Category category)
        {
            return CreateVoid.CreateResult(category, () => CodeBase.CreateDumpPrintText(_value.DumpPrintText));
        }
    }
}