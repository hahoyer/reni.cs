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
        [DumpData(true)]
        private readonly TypeBase _value;

        internal readonly IFeature DumpPrintFeature;
        private static readonly IFunctionalFeature _functionalFeature = new FunctionalFeature();

        private class FunctionalFeature : IFunctionalFeature
        {
            string IDumpShortProvider.DumpShort() { return "type"; }
            Result IFunctionalFeature.Apply(Category category, Result functionResult, Result argsResult) { return argsResult.ConvertTo(functionResult.Type.StripFunctional().AutomaticDereference()) & category; }
        }

        private class DumpPrintFeatureImplementation : ReniObject, IFeature
        {
            private readonly TypeType _parent;
            public DumpPrintFeatureImplementation(TypeType parent) { _parent = parent; }

            TypeBase IFeature.DefiningType() { return _parent; }

            Result IFeature.Apply(Category category) { return Void.CreateResult(category, () => CodeBase.CreateDumpPrintText(_parent._value.DumpPrintText)); }
        }

        public TypeType(TypeBase value)
        {
            _value = value;
            DumpPrintFeature = new DumpPrintFeatureImplementation(this);
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }
        protected internal override Result ConvertToItself(Category category) { return CreateVoidResult(category); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _functionalFeature; }
        internal override TypeBase StripFunctional() { return _value; }
        internal override string DumpShort() { return "(" + _value.DumpShort() + ") type"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }
    }
}