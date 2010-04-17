using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

#pragma warning disable 1911

namespace Reni.Type
{
    internal sealed class Reference: TypeBase
    {
        private readonly TypeBase _target;
        private readonly RefAlignParam _refAlignParam;
        
        internal Reference(TypeBase target, RefAlignParam refAlignParam)
        {
            _target = target;
            _refAlignParam = refAlignParam;
        }

        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        protected override Size GetSize() { return _refAlignParam.RefSize; }

        internal override string DumpShort() { return "reference("+_target.DumpShort()+")"; }
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

        internal override sealed Result AutomaticDereference(Result result)
        {
            return Parent
                .AutomaticDereference(CreateDereferencedArgResult(result.CompleteCategory).UseWithArg(result));
        }

        private Result CreateDereferencedArgResult(Category category)
        {
            return Parent
                .CreateResult
                (
                category,
                CreateDereferencedArgCode
                );
        }

        private CodeBase CreateDereferencedArgCode() { return CodeBase.CreateArg(Size).CreateDereference(RefAlignParam, Parent.Size); }

        internal override sealed TypeBase AutomaticDereference() { return Parent.AutomaticDereference(); }

        internal override sealed bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        [DumpData(false)]
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        [DumpData(false)]
        internal TypeBase AlignedTarget { get { return Parent.CreateAlign(RefAlignParam.AlignBits); } }

        internal override sealed string DumpShort() { return ShortName + "." + Parent.DumpShort(); }

        protected Result ConvertTo_Implementation<TRefType>(Category category, TypeBase dest)
            where TRefType : Ref
        {
            var destAsRef = dest as TRefType;
            if (destAsRef != null && RefAlignParam == destAsRef.RefAlignParam && Parent == destAsRef.Parent)
                return dest.ConvertToItself(category);

            return Parent
                .ConvertTo(category, dest)
                .UseWithArg(CreateDereferencedArgResult(category));
        }

        internal override sealed Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }

        protected bool IsConvertableTo_Implementation<TRefType>(TypeBase dest, ConversionFeature conversionFeature)
            where TRefType : Ref
        {
            var destAsRef = dest as TRefType;
            if (destAsRef != null && RefAlignParam == destAsRef.RefAlignParam && Parent == destAsRef.Parent)
                return true;

            return Parent
                .IsConvertableTo(dest, conversionFeature);
        }

        internal override bool IsRefLike(Ref target) { return Parent == target.Parent && RefAlignParam == target.RefAlignParam; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Parent.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        protected override bool IsInheritor { get { return true; } }

        internal override TypeBase GetEffectiveType() { return Parent.GetEffectiveType(); }
    }

    internal interface IRef
    {
        IFeature GetDumpPrintFeature();
    }
}