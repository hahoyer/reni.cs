using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AutomaticReferenceType : TypeBase, IReference
    {
        private static int _nextObjectId;
        private readonly RefAlignParam _refAlignParam;
        private readonly TypeBase _valueType;

        internal AutomaticReferenceType(TypeBase valueType, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            Tracer.Assert(!valueType.Size.IsZero, valueType.Dump);
            Tracer.Assert(!(valueType is AutomaticReferenceType), valueType.Dump);
            _valueType = valueType;
            _refAlignParam = refAlignParam;
            StopByObjectId(-2);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

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

        internal TypeBase ValueType { get { return _valueType; } }
        internal override string DumpPrintText { get { return DumpShort(); } }

        internal Result ObjectReferenceInCode(Category category)
        {
            var objectRef = ObjectReference(RefAlignParam);
            var destinationResult = Result
                (
                    category,
                    () => CodeBase.ReferenceCode(objectRef).Dereference(RefAlignParam, Size),
                    () => Refs.Create(objectRef)
                );
            return destinationResult;
        }

        internal override Structure GetStructure() { return ValueType.GetStructure(); }

        internal override Result ReferenceInCode(IReferenceInCode target, Category category)
        {
            return Result
                (
                    category,
                    () => CodeBase.ReferenceCode(target).Dereference(target.RefAlignParam, target.RefAlignParam.RefSize),
                    () => Refs.Create(target)
                );
        }

        internal override TypeBase ToReference(RefAlignParam refAlignParam) { return this; }

        protected override Size GetSize() { return RefAlignParam.RefSize; }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _valueType.Search(searchVisitor.Child(this));
            _valueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return ValueType.IsConvertableTo(dest, conversionParameter); }
        internal override TypeBase TypeForTypeOperator() { return ValueType.TypeForTypeOperator(); }
    }
}