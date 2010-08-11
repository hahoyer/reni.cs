using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;

namespace Reni.Type
{
    sealed internal class FunctionAccessType : TypeBase
    {
        [DumpData(true)]
        private readonly TypeBase _objectType;

        [DumpData(true)]
        private readonly IFunctionalFeature _functionalFeature;


        public FunctionAccessType(TypeBase objectType, IFunctionalFeature functionalFeature)
        {
            Tracer.Assert(!(objectType is Reference));
            _functionalFeature = functionalFeature;
            _objectType = objectType;
            StopByObjectId(-369);
        }

        protected override Size GetSize() { return _objectType.Size; }
        internal override string DumpPrintText { get { return _functionalFeature.DumpShort(); } }
        internal override string DumpShort() { return _objectType.DumpShort() + " " + _functionalFeature.DumpShort(); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _functionalFeature; }
        protected override TypeBase GetObjectType() { return _objectType; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal Result ContextOperatorFeatureApply(Category category) { return _functionalFeature.ContextOperatorFeatureApply(category); }
    }

    internal class FunctionDefinitionType : TypeBase
    {
        private readonly FunctionalFeature _functionalFeature;
        public FunctionDefinitionType(FunctionalFeature functionalFeature)
        {
            _functionalFeature = functionalFeature;
            StopByObjectId(-191);
        }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpShort() { return _functionalFeature.DumpShort(); }

        internal override TypeBase AccessType(Struct.Context context, int position)
        {
            return context
                .ContextType
                .CreateFunctionalType(_functionalFeature)
                .CreateReference(context.RefAlignParam);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        public Result CreateDumpPrintResult(Category category)
        {
            return CreateVoid.CreateResult(category, () => CodeBase.CreateDumpPrintText(_functionalFeature.DumpPrintText));
        }
    }

}