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
        private readonly TypeBase _target;
        private readonly RefAlignParam _refAlignParam;
        private readonly SimpleCache<TypeType> _typeTypeCache;

        internal Reference(TypeBase target, RefAlignParam refAlignParam)
            :base(_nextObjectId++)
        {
            Tracer.Assert(!(target is Reference));
            _target = target;
            _refAlignParam = refAlignParam;
            _typeTypeCache = new SimpleCache<TypeType>(() => new TypeType(this));
        }

        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal TypeBase Target { get { return _target; } }

        [IsDumpEnabled(false)]
        internal TypeBase AlignedTarget { get { return _target.Align(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override string DumpPrintText { get { return DumpShort(); } }
        internal override string DumpShort() { return "reference(" + _target.DumpShort() + ")"; }
        internal override int SequenceCount(TypeBase elementType) { return _target.SequenceCount(elementType); }
        internal override IFunctionalFeature FunctionalFeature() { return Target.FunctionalFeature(); }
        internal override TypeBase ObjectType() { return Target.ObjectType(); }
        internal override bool IsRefLike(Reference target) { return Target == target.Target && RefAlignParam == target.RefAlignParam; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _target.Search(searchVisitor.Child(this));
            _target.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override Struct.Context GetStruct() { return _target.GetStruct(); }

        protected override bool IsReferenceTo(TypeBase target) { return Target == target; }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter)
        {
            return _target.IsConvertableTo(dest, conversionParameter);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var trace = ObjectId == -13 && category.HasCode && dest.ObjectId == 464;
            StartMethodDump(trace,category,dest);
            var convertTo = _target.ConvertTo(category, dest);
            var dereferencedArgResult = DereferencedArgResult(category);
            DumpWithBreak(trace,"convertTo",convertTo,"dereferencedArgResult",dereferencedArgResult);
            return ReturnMethodDump(trace, convertTo.ReplaceArg(dereferencedArgResult));
        }

        internal override TypeBase AutomaticDereference() { return _target.AutomaticDereference(); }
        internal override TypeBase TypeForTypeOperator() { return _target.TypeForTypeOperator(); }
        protected override TypeBase Dereference() { return Target; }
        protected override Result DereferenceResult(Category category) { return DereferencedArgResult(category); }

        [IsDumpEnabled(false)]
        internal TypeType TypeType { get { return _typeTypeCache.Value; } }

        private Result DereferencedArgResult(Category category) { return _target.Result(category, DereferencedArgCode); }
        private CodeBase DereferencedArgCode() { return CodeBase.Arg(Size).Dereference(RefAlignParam, _target.Size); }
        private Result ObjectRefInCode(Category category) { return Target.ObjectRefInCode(category, RefAlignParam); }
    }
}