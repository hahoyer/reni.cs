using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    internal abstract class Ref : Child 
    {
        static int _nextObjectId;
        internal RefAlignParam RefAlignParam;

        protected Ref(TypeBase target, RefAlignParam refAlignParam) : base(_nextObjectId, target)
        {
            RefAlignParam = refAlignParam;
        }

        [DumpData(false)]
        internal TypeBase Target { get { return Parent; } }
        sealed internal override Size Size { get { return RefAlignParam.RefSize; } }
        [DumpData(false)]
        sealed internal override string DumpPrintText { get { return "#(#" + ShortName + "#)# " + Parent.DumpPrintText; } }
        [DumpData(false)]
        abstract protected string ShortName { get; }
        [DumpData(false)]
        sealed internal override int SequenceCount { get { return Target.SequenceCount; } }

        sealed internal override Result DestructorHandler(Category category)
        {
            return EmptyHandler(category);
        }

        sealed internal override Result MoveHandler(Category category)
        {
            return EmptyHandler(category);
        }

        sealed internal override Result Dereference(Result result)
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

        sealed internal override TypeBase Dereference()
        {
            Tracer.Assert(Target == Target.Dereference());
            return Target;
        }

        sealed internal override Result DumpPrint(Category category)
        {
            return Target.DumpPrintFromRef(category, RefAlignParam);
        }

        sealed internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam == RefAlignParam);
            return DumpPrint(category);
        }
        [DumpData(false)]
        sealed internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        sealed internal override string DumpShort()
        {
            return ShortName + "." + Parent.DumpShort();
        }

        sealed internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return Target
                .ConvertTo(category, dest)
                .UseWithArg(CreateDereferencedArgResult(category));
        }

        sealed internal override Result ApplyTypeOperator(Result argResult)
        {
            return Target.ApplyTypeOperator(argResult);
        }

        sealed internal override Result TypeOperator(Category category)
        {
            return Target.TypeOperator(category);
        }

        sealed internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Target.IsConvertableTo(dest, conversionFeature);
        }

    }

    internal sealed class AutomaticRef : Ref
    {
        internal AutomaticRef(TypeBase target, RefAlignParam refAlignParam) : base(target,refAlignParam)
        {
        }

        public override AutomaticRef CreateRef(RefAlignParam refAlignParam)
        {
            NotImplementedMethod(refAlignParam);
            return null;
        }

        internal override Result PostProcess(AutomaticRef visitedType, Result result)
        {
            if(this == visitedType)
                return result;
            return base.PostProcess(visitedType, result);
        }

        protected override string ShortName { get { return "automatic_ref"; } }
    }
}