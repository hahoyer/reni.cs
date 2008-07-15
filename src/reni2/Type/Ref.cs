using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;

namespace Reni.Type
{
    internal abstract class Ref : Child
    {
        private static int _nextObjectId;
        internal RefAlignParam RefAlignParam;

        protected Ref(TypeBase target, RefAlignParam refAlignParam) : base(_nextObjectId++, target)
        {
            RefAlignParam = refAlignParam;
        }

        [DumpData(false)]
        internal TypeBase Target { get { return Parent; } }
        internal override sealed Size Size { get { return RefAlignParam.RefSize; } }
        [DumpData(false)]
        internal override sealed string DumpPrintText { get { return "#(#" + ShortName + "#)# " + Parent.DumpPrintText; } }
        [DumpData(false)]
        protected abstract string ShortName { get; }
        [DumpData(false)]
        internal override sealed int SequenceCount { get { return Target.SequenceCount; } }

        internal override sealed Result DestructorHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal override sealed Result MoveHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal override sealed Result AutomaticDereference(Result result)
        {
            return CreateDereferencedArgResult(result.Complete).UseWithArg(result);
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
            Tracer.Assert(Target == Target.AutomaticDereference());
            return Target;
        }

        internal override sealed Result DumpPrint(Category category)
        {
            return Target.DumpPrintFromRef(category, RefAlignParam);
        }

        internal override sealed Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam == RefAlignParam);
            return DumpPrint(category);
        }

        [DumpData(false)]
        internal override sealed bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override Size UnrefSize
        {
            get { return Target.UnrefSize; }
        }

        internal override sealed string DumpShort()
        {
            return ShortName + "." + Parent.DumpShort();
        }

        internal override sealed Result ConvertToVirt(Category category, TypeBase dest)
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

        internal override sealed bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Target.IsConvertableTo(dest, conversionFeature);
        }

        internal override Result AccessResult(Category category, int index)
        {
            return Target.AccessResultFromRef(category, index,RefAlignParam);
        }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            var resultFromRef = Parent.SearchFromRef(defineable).SubTrial(Parent);
            var result = resultFromRef.SearchResultDescriptor.Convert(resultFromRef.Feature, this);
            if(result.IsSuccessFull)
                return result;
            result = Parent.Search(defineable).AlternativeTrial(result);
            if(result.IsSuccessFull)
                return result;

            return base.Search(defineable).AlternativeTrial(result);
        }
    }

    internal sealed class AutomaticRef : Ref
    {
        internal AutomaticRef(TypeBase target, RefAlignParam refAlignParam) : base(target, refAlignParam) {}

        public override AutomaticRef CreateRef(RefAlignParam refAlignParam)
        {
            NotImplementedMethod(refAlignParam);
            return null;
        }

        protected override string ShortName { get { return "automatic_ref"; } }
    }
}