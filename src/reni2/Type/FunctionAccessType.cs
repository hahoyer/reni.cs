using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    internal class FunctionAccessType : TypeBase
    {
        [DumpData(true)]
        private readonly TypeBase _objectType;

        [DumpData(true)]
        private readonly IFunctionalFeature _feature;

        public FunctionAccessType(TypeBase objectType, IFunctionalFeature feature)
        {
            _objectType = objectType;
            _feature = feature;
        }

        protected override Size GetSize() { return _objectType.Size; }
        internal override string DumpShort() { return _objectType.DumpShort() + " " + _feature.DumpShort(); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _feature; }
        internal override TypeBase StripFunctional() { return _objectType; }
    }

    internal class FunctionDefinitionType : TypeBase
    {
        [DumpData(true)]
        private readonly IFunctionalFeature _functionalFeature;

        public FunctionDefinitionType(IFunctionalFeature functionalFeature)
        {
            _functionalFeature = functionalFeature;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpShort() { return _functionalFeature.DumpShort(); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _functionalFeature; }

        internal Result ContextOperatorFeatureApply(Category category)
        {
            return _functionalFeature.ContextOperatorFeatureApply(category);
        }

        internal override string DumpPrintText { get { return _functionalFeature.DumpShort(); } }
        public Result CreateDumpPrintResult(Category category) { return _functionalFeature.DumpPrintFeatureApply(category); }
    }

}