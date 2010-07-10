using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    internal sealed class Reference: TypeBase
    {
        private static int _nextObjectId;
        private readonly TypeBase _target;
        private readonly RefAlignParam _refAlignParam;
        
        internal Reference(TypeBase target, RefAlignParam refAlignParam)
            :base(_nextObjectId++)
        {
            _target = target;
            _refAlignParam = refAlignParam;
        }

        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        internal TypeBase Target { get { return _target; } }

        [DumpData(false)]
        internal TypeBase AlignedTarget { get { return _target.CreateAlign(RefAlignParam.AlignBits); } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        internal override sealed bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override string DumpShort() { return "reference(" + _target.DumpShort() + ")"; }
        internal override int GetSequenceCount(TypeBase elementType) { return _target.GetSequenceCount(elementType); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _target.Search(searchVisitor.Child(this));
            _target.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override Struct.Context GetStruct() { return _target.GetStruct(); }

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
                .UseWithArg(CreateDereferencedArgResult(category));
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

        internal override sealed Result AutomaticDereference(Result result)
        {
            Result useWithArg = CreateDereferencedArgResult(result.CompleteCategory).UseWithArg(result);
            return _target
                .AutomaticDereference(useWithArg);
        }
    }

    [Obsolete("",true)]
    internal abstract class Ref : Child
    {
        private static int _nextObjectId;
        internal readonly RefAlignParam RefAlignParam;

        protected Ref(TypeBase target, RefAlignParam refAlignParam)
            : base(_nextObjectId++, target)
        {
            RefAlignParam = refAlignParam;
            StopByObjectId(-6);
        }

        protected override sealed Size GetSize() { return RefAlignParam.RefSize; }

        [DumpData(false)]
        internal override sealed string DumpPrintText { get { return "#(#" + ShortName + "#)# " + Parent.DumpPrintText; } }

        [DumpData(false)]
        protected abstract string ShortName { get; }

        internal override sealed int GetSequenceCount(TypeBase elementType) { return Parent.GetSequenceCount(elementType); }

        internal override sealed Result Destructor(Category category) { return CreateVoidCodeAndRefs(category); }

        internal override sealed Result Copier(Category category) { return CreateVoidCodeAndRefs(category); }

        internal override sealed TypeBase AutomaticDereference() { return Parent.AutomaticDereference(); }

        [DumpData(false)]
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        internal override sealed string DumpShort() { return ShortName + "." + Parent.DumpShort(); }

        internal override sealed Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }

        internal override bool IsRefLike(Reference target) { return Parent == target.Target && RefAlignParam == target.RefAlignParam; }

        protected override bool IsInheritor { get { return true; } }

        internal override TypeBase GetEffectiveType() { return Parent.GetEffectiveType(); }
    }

    internal interface IRef
    {
        IFeature GetDumpPrintFeature();
    }
}