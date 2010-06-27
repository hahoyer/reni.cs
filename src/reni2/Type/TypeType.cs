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

        private static readonly IFunctionalFeature _functionalFeature = new FunctionalFeature();

        private class FunctionalFeature : IFunctionalFeature
        {
            string IDumpShortProvider.DumpShort() { return "type"; }
            Result IFunctionalFeature.Apply(Category category, Result functionResult, Result argsResult) { return argsResult.ConvertTo(functionResult.Type.StripFunctional().AutomaticDereference()) & category; }
            Result IFunctionalFeature.ContextOperatorFeatureApply(Category category) { throw new NotImplementedException(); }
        }

        public TypeType(TypeBase value)
        {
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

        internal Result ApplyDumpPrintFeature(Category category) { return CreateVoid.CreateResult(category, () => CodeBase.CreateDumpPrintText(_value.DumpPrintText)); }
    }
}