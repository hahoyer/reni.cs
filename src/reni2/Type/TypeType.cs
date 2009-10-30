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
        private static readonly IFunctionalFeature _functionalFeature = new FunctionalFeatureImplementation();

        private class FunctionalFeatureImplementation : IFunctionalFeature
        {
            string IDumpShortProvider.DumpShort() { return "type"; }
            Result IFunctionalFeature.Apply(Category category, Result functionResult, Result argsResult)
            {
                return argsResult.ConvertTo(functionResult.Type.StripFunctional().AutomaticDereference()) & category;
            }
        }

        private class DumpPrintFeatureImplementation : ReniObject, IFeature
        {
            private readonly TypeBase _value;
            public DumpPrintFeatureImplementation(TypeBase value) { _value = value; }

            TypeBase IFeature.DefiningType()
            {
                NotImplementedMethod();
                return null;
            }

            Result IFeature.Apply(Category category) { return Void.CreateResult(category, () => CodeBase.CreateDumpPrintText(_value.DumpPrintText)); }
        }

        public TypeType(TypeBase value)
        {
            DumpPrintFeature = new DumpPrintFeatureImplementation(value);
            _value = value;
        }

        internal override bool IsValidRefTarget() { return false; }
        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpPrintText { get { return "(" + _value.DumpPrintText + "()) type"; } }
        protected override Result ConvertToItself(Category category) { return CreateVoidResult(category); }
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