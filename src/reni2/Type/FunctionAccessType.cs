using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    internal class FunctionAccessType : TypeBase
    {
        [DumpData(true)]
        private readonly TypeBase _objectType;

        [DumpData(true)]
        private readonly IFunctionalFeature _functionalFeature;

        public FunctionAccessType(TypeBase objectType, IFunctionalFeature functionalFeature)
        {
            _objectType = objectType;
            _functionalFeature = functionalFeature;
            StopByObjectId(51754);
        }

        internal IFunctionalFeature FunctionalFeature { get { return _functionalFeature; } }
        internal TypeBase ObjectType { get { return _objectType; } }

        protected override Size GetSize() { return _objectType.Size; }
        internal override string DumpPrintText { get { return _functionalFeature.DumpShort(); } }

        internal Result ContextOperatorFeatureApply(Category category) { return _functionalFeature.ContextOperatorFeatureApply(category); }
        public Result CreateDumpPrintResult(Category category) { return _functionalFeature.DumpPrintFeatureApply(category); }
        internal override string DumpShort() { return _objectType.DumpShort() + " " + _functionalFeature.DumpShort(); }
        internal override IFunctionalFeature GetFunctionalFeature() { return _functionalFeature; }
        internal override TypeBase StripFunctional() { return _objectType; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal override Result Apply(Category category, Func<Category, Result> right, RefAlignParam refAlignParam)
        {
            return _functionalFeature
                .Apply(category, right(Category.Type).Type, refAlignParam)
                .ReplaceArg(right(category))
                .ReplaceObjectRefByArg(refAlignParam, _objectType);
        }
    }

    internal class FunctionDefinitionType : TypeBase
    {
        private readonly Function _function;
        public FunctionDefinitionType(Function function) { _function = function; }

        protected override Size GetSize() { return Size.Zero; }
        internal override string DumpShort() { return _function.DumpShort(); }

        internal override TypeBase AccessType(Struct.Context context, int position)
        {
            return context
                .ContextReferenceType
                .CreateFunctionalType(_function);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        public Result CreateDumpPrintResult(Category category)
        {
            return CreateVoid.CreateResult(category, () => CodeBase.CreateDumpPrintText(_function.DumpPrintText));
        }
    }

}