using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class Reference : TypeBase
    {
        private static int _nextObjectId;
        private readonly TypeBase _valueType;
        private readonly RefAlignParam _refAlignParam;
        private readonly AssignmentFeature _assignmentFeature;
        private readonly DictionaryEx<AssignmentFeature, TypeBase> _functionalTypeCache;

        internal Reference(TypeBase valueType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            Tracer.Assert(!(valueType is Reference), valueType.Dump);
            _valueType = valueType;
            _refAlignParam = refAlignParam;
            _assignmentFeature = new AssignmentFeature(this);
            _functionalTypeCache = new DictionaryEx<AssignmentFeature, TypeBase>(assignmentFeature => new FunctionalType(this, assignmentFeature));
        }

        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal TypeBase ValueType { get { return _valueType; } }

        [DisableDump]
        internal TypeBase AlignedTarget { get { return _valueType.Align(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override string DumpPrintText { get { return DumpShort(); } }
        internal override string DumpShort() { return "reference(" + _valueType.DumpShort() + ")"; }
        internal override int SequenceCount(TypeBase elementType) { return _valueType.SequenceCount(elementType); }
        internal override IFunctionalFeature FunctionalFeature() { return ValueType.FunctionalFeature(); }
        internal override TypeBase ObjectType() { return ValueType.ObjectType(); }
        protected override bool IsRefLike(Reference target) { return ValueType == target.ValueType && RefAlignParam == target.RefAlignParam; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _valueType.Search(searchVisitor.Child(this));
            _valueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override AccessPoint GetStructAccessPoint() { return _valueType.GetStructAccessPoint(); }

        protected override bool IsReferenceTo(TypeBase value) { return ValueType == value; }

        [DisableDump]
        internal override RefAlignParam[] ReferenceChain
        {
            get
            {
                var subResult = ValueType.ReferenceChain;
                var result = new RefAlignParam[subResult.Length + 1];
                result[0] = RefAlignParam;
                subResult.CopyTo(result, 1);
                return result;
            }
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return _valueType.IsConvertableTo(dest, conversionParameter); }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var trace = ObjectId == -13 && category.HasCode && dest.ObjectId == 464;
            StartMethodDump(trace, category, dest);
            var convertTo = _valueType.ConvertTo(category, dest);
            var dereferencedArgResult = DereferencedArgResult(category);
            DumpWithBreak(trace, "convertTo", convertTo, "dereferencedArgResult", dereferencedArgResult);
            return ReturnMethodDump(trace, convertTo.ReplaceArg(dereferencedArgResult));
        }

        internal override TypeBase AutomaticDereference() { return _valueType.AutomaticDereference(); }
        internal override TypeBase TypeForTypeOperator() { return _valueType.TypeForTypeOperator(); }
        protected override TypeBase Dereference() { return ValueType; }
        protected override Result DereferenceResult(Category category) { return DereferencedArgResult(category); }

        private Result DereferencedArgResult(Category category) { return _valueType.Result(category, DereferencedArgCode); }
        private CodeBase DereferencedArgCode() { return CodeBase.Arg(Size).Dereference(RefAlignParam, _valueType.Size); }

        internal Result ApplyAssignment(Category category)
        {
            return FunctionalType(_assignmentFeature)
                .ArgResult(category);
        }

        private TypeBase FunctionalType(AssignmentFeature assignmentFeature) { return _functionalTypeCache.Find(assignmentFeature); }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var result = Void
                .Result
                (
                    category,
                    () => CodeBase.Arg(RefAlignParam.RefSize*2).CreateAssignment(RefAlignParam, ValueType.Size)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsType
                .ConvertTo(category | Category.Type, ValueType)
                .LocalReferenceResult(RefAlignParam);
            var objectRef = ObjectReference(RefAlignParam);
            var destinationResult = Result
                (
                    category,
                    () => CodeBase.ReferenceInCode(objectRef),
                    () => Refs.Create(objectRef)
                );
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.ReplaceArg(objectAndSourceRefs);
        }

        internal override Result ReferenceInCode(ContextBase function, Category category)
        {
            return Result
                (
                    category,
                    () => CodeBase.ReferenceInCode(function).Dereference(function.RefAlignParam, function.RefAlignParam.RefSize),
                    () => Refs.Create(function)
                )
                ;
        }
    }

    internal sealed class FunctionalType : TypeBase
    {
        private readonly Reference _parent;
        private readonly AssignmentFeature _functionalFeature;

        public FunctionalType(Reference parent, AssignmentFeature functionalFeature) 
        {
            _parent = parent;
            _functionalFeature = functionalFeature;
        }

        protected override Size GetSize()
        {
            NotImplementedMethod();
            return null;
        }

        internal override string DumpShort() { return "(" + _parent.DumpShort() + " " + _functionalFeature.DumpShort() + ")"; }
    }
    
    internal class FunctionalFeature
    {}
}