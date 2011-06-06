using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Type
{
    internal sealed class FunctionAccessType : TypeBase, IAccessType
    {
        [IsDumpEnabled(true)]
        private readonly TypeBase _objectType;

        [IsDumpEnabled(true)]
        private readonly IFunctionalFeature _functionalFeature;

        private static int _nextObjectId;


        public FunctionAccessType(TypeBase objectType, IFunctionalFeature functionalFeature)
            : base(_nextObjectId++)
        {
            _functionalFeature = functionalFeature;
            _objectType = objectType;
            StopByObjectId(-325);
        }

        Result IAccessType.Result(Category category) { return ArgResult(category); }

        protected override Size GetSize() { return _objectType.Size; }
        internal override string DumpPrintText { get { return _functionalFeature.DumpShort(); } }
        internal override string DumpShort() { return _objectType.DumpShort() + " " + _functionalFeature.DumpShort(); }
        internal override IFunctionalFeature FunctionalFeature() { return _functionalFeature; }
        internal override TypeBase ObjectType() { return _objectType; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result ContextOperatorFeatureApply(Category category) { return _functionalFeature.ContextOperatorFeatureApply(category); }
    }
}