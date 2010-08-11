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

        [DumpData(false)]
        internal TypeBase AlignedTarget { get { return _target.CreateAlign(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override string DumpShort() { return "reference(" + _target.DumpShort() + ")"; }
        internal override int GetSequenceCount(TypeBase elementType) { return _target.GetSequenceCount(elementType); }

        internal override bool IsRefLike(Reference target) { return Target == target.Target && RefAlignParam == target.RefAlignParam; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _target.Search(searchVisitor.Child(this));
            _target.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override Struct.Context GetStruct() { return _target.GetStruct(); }

        protected override bool IsReferenceTo(TypeBase target) { return Target == target; }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            var destAsRef = dest as Reference;
            if (destAsRef != null && RefAlignParam == destAsRef.RefAlignParam && _target == destAsRef._target)
                return true;

            return _target.IsConvertableTo(dest, conversionFeature);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            var destAsRef = dest as Reference;
            if (destAsRef != null && RefAlignParam == destAsRef.RefAlignParam && _target == destAsRef._target)
                return dest.ConvertToItself(category);

            return _target
                .ConvertTo(category, dest)
                .ReplaceArg(CreateDereferencedArgResult(category));
        }

        private Result CreateDereferencedArgResult(Category category)
        {
            return _target
                .CreateResult
                (
                category,
                CreateDereferencedArgCode
                );
        }

        private CodeBase CreateDereferencedArgCode()
        {
            return CodeBase
                .CreateArg(Size)
                .CreateDereference(RefAlignParam, _target.Size);
        }

        internal override TypeBase AutomaticDereference() { return _target.AutomaticDereference(); }
        internal override TypeBase GetTypeForTypeOperator() { return _target.GetTypeForTypeOperator(); }

        internal override Result AutomaticDereference(Result result)
        {
            Result useWithArg = CreateDereferencedArgResult(result.CompleteCategory).ReplaceArg(result);
            return _target
                .AutomaticDereference(useWithArg);
        }

        private Result CreateObjectRefInCode(Category category) { return Target.CreateObjectRefInCode(category, RefAlignParam); }

        internal Result CreateResult(Category category, bool isContextFeature)
        {
            if(isContextFeature)
                return CreateObjectRefInCode(category);
            return CreateArgResult(category);
        }

        [DumpData(false)]
        internal TypeType TypeType { get { return _typeTypeCache.Value; } }

    }
}