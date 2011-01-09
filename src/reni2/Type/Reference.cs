using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    internal sealed class Reference: TypeBase
    {
        private static int _nextObjectId;
        private readonly TypeBase _value;
        private readonly RefAlignParam _refAlignParam;
        private readonly SimpleCache<TypeType> _typeTypeCache;

        internal Reference(TypeBase value, RefAlignParam refAlignParam)
            :base(_nextObjectId++)
        {
            Tracer.Assert(!(value is Reference));
            _value = value;
            _refAlignParam = refAlignParam;
            _typeTypeCache = new SimpleCache<TypeType>(() => new TypeType(this));
        }

        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal TypeBase Value { get { return _value; } }

        [IsDumpEnabled(false)]
        internal TypeBase AlignedTarget { get { return _value.Align(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override string DumpPrintText { get { return DumpShort(); } }
        internal override string DumpShort() { return "reference(" + _value.DumpShort() + ")"; }
        internal override int SequenceCount(TypeBase elementType) { return _value.SequenceCount(elementType); }
        internal override IFunctionalFeature FunctionalFeature() { return Value.FunctionalFeature(); }
        internal override TypeBase ObjectType() { return Value.ObjectType(); }
        internal override bool IsRefLike(Reference target) { return Value == target.Value && RefAlignParam == target.RefAlignParam; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _value.Search(searchVisitor.Child(this));
            _value.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override Struct.Context GetStruct() { return _value.GetStruct(); }

        protected override bool IsReferenceTo(TypeBase value) { return Value == value; }

        internal override RefAlignParam[] ReferenceChain
        {
            get
            {
                var subResult = Value.ReferenceChain;
                var result = new RefAlignParam[subResult.Length + 1];
                result[0] = RefAlignParam;
                subResult.CopyTo(result, 1);
                return result;
            }
        }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            return _value.IsConvertableTo(dest, conversionParameter);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var trace = ObjectId == -13 && category.HasCode && dest.ObjectId == 464;
            StartMethodDump(trace,category,dest);
            var convertTo = _value.ConvertTo(category, dest);
            var dereferencedArgResult = DereferencedArgResult(category);
            DumpWithBreak(trace,"convertTo",convertTo,"dereferencedArgResult",dereferencedArgResult);
            return ReturnMethodDump(trace, convertTo.ReplaceArg(dereferencedArgResult));
        }

        internal override TypeBase AutomaticDereference() { return _value.AutomaticDereference(); }
        internal override TypeBase TypeForTypeOperator() { return _value.TypeForTypeOperator(); }
        protected override TypeBase Dereference() { return Value; }
        protected override Result DereferenceResult(Category category) { return DereferencedArgResult(category); }

        [IsDumpEnabled(false)]
        internal TypeType TypeType { get { return _typeTypeCache.Value; } }

        private Result DereferencedArgResult(Category category) { return _value.Result(category, DereferencedArgCode); }
        private CodeBase DereferencedArgCode() { return CodeBase.Arg(Size).Dereference(RefAlignParam, _value.Size); }
        private Result ObjectRefInCode(Category category) { return Value.ObjectRefInCode(category, RefAlignParam); }
    }
}