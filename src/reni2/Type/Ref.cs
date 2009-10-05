using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;

#pragma warning disable 1911

namespace Reni.Type
{
    [Serializable]
    internal abstract class Ref : Child
    {
        private static int _nextObjectId;
        internal readonly RefAlignParam RefAlignParam;

        protected Ref(TypeBase target, RefAlignParam refAlignParam)
            : base(_nextObjectId++, target)
        {
            Tracer.Assert(!(target is Aligner));
            RefAlignParam = refAlignParam;
            StopByObjectId(-6);
        }

        [DumpData(false)]
        internal TypeBase Target { get { return Parent; } }

        protected override sealed Size GetSize()
        {
            return RefAlignParam.RefSize;
        }

        [DumpData(false)]
        internal override sealed string DumpPrintText { get { return "#(#" + ShortName + "#)# " + Parent.DumpPrintText; } }

        [DumpData(false)]
        protected abstract string ShortName { get; }

        [DumpData(false)]
        internal override sealed int SequenceCount { get { return Target.SequenceCount; } }

        internal override sealed Result Destructor(Category category)
        {
            return CreateVoidCodeAndRefs(category);
        }

        internal override sealed Result Copier(Category category)
        {
            return CreateVoidCodeAndRefs(category);
        }

        internal override sealed Result AutomaticDereference(Result result)
        {
            return Target.AutomaticDereference(CreateDereferencedArgResult(result.CompleteCategory).UseWithArg(result));
        }

        internal Result CreateDereferencedArgResult(Category category)
        {
            return Target.CreateResult
                (
                category,
                CreateDereferencedArgCode
                );
        }

        private CodeBase CreateDereferencedArgCode()
        {
            return CodeBase.CreateArg(Size).CreateDereference(RefAlignParam, Target.Size);
        }

        internal override sealed TypeBase AutomaticDereference()
        {
            return Target.AutomaticDereference();
        }

        internal new Result DumpPrint(Category category)
        {
            return Target.DumpPrintFromRef(category, RefAlignParam);
        }

        internal override sealed Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam == RefAlignParam);
            return DumpPrint(category);
        }

        internal override sealed bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        [DumpData(false)]
        internal override Size UnrefSize { get { return Target.UnrefSize; } }

        [DumpData(false)]
        internal TypeBase AlignedTarget { get { return Target.CreateAlign(RefAlignParam.AlignBits); } }

        internal override sealed string DumpShort()
        {
            return ShortName + "." + Parent.DumpShort();
        }

        protected override sealed Result ConvertToImplementation(Category category, TypeBase dest)
        {
            return Target
                .ConvertTo(category, dest)
                .UseWithArg(CreateDereferencedArgResult(category));
        }

        internal override sealed Result ApplyTypeOperator(Result argResult)
        {
            return Target.ApplyTypeOperator(argResult);
        }

        internal override sealed Result TypeOperator(Category category)
        {
            return Target.TypeOperator(category);
        }

        internal override sealed bool IsConvertableToImplementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Target.IsConvertableTo(dest, conversionFeature);
        }

        internal override Result AccessResultAsArg(Category category, int position)
        {
            return Target.AccessResultAsArgFromRef(category, position, RefAlignParam);
        }

        internal Result AccessResultAsContextRef(Category category, int position)
        {
            return Target.AccessResultAsContextRefFromRef(category, position, RefAlignParam);
        }

        internal override bool IsRefLike(Ref target)
        {
            return Target == target.Target && RefAlignParam == target.RefAlignParam;
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Parent.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        protected override bool IsInheritor { get { return true; } }

        internal Result CreateContextResult(IRefInCode context, Category category)
        {
            return CreateResult(
                category,
                () => CodeBase.CreateContextRef(context).CreateRefPlus(context.RefAlignParam, Target.Size*-1),
                () => Refs.Context(context));
        }
    }

    [Serializable]
    internal sealed class AutomaticRef : Ref
    {
        internal AutomaticRef(TypeBase target, RefAlignParam refAlignParam)
            : base(target, refAlignParam)
        {
        }

        protected override string ShortName { get { return "automatic_ref"; } }
    }
}