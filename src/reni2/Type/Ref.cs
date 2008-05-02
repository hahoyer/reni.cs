using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper.TreeViewSupport;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class Ref : Child
    {
        private static int _nextObjectId;
        private readonly AssignmentOperatorFeature _assignmentOperatorFeatureObject;
        private readonly RefAlignParam _refAlignParam;

        public Ref(TypeBase target, RefAlignParam refAlignParam) : base(_nextObjectId++, target)
        {
            _refAlignParam = refAlignParam;
            _assignmentOperatorFeatureObject =
                new AssignmentOperatorFeature(this);
        }

        [Node, DumpData(false)]
        public RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DumpData(false)]
        public TypeBase Target { get { return Parent; } }

        public override Size Size { get { return RefAlignParam.RefSize; } }

        [DumpData(false)]
        public override bool IsRef { get { return true; } }

        public override Size UnrefSize { get { return Target.Size; } }

        [DumpData(false)]
        internal override string DumpPrintText { get { return "#(#ref#)# " + Parent.DumpPrintText; } }

        [DumpData(false)]
        internal override int SequenceCount { get { return Target.SequenceCount; } }

        internal override Result DestructorHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal override Result MoveHandler(Category category)
        {
            return EmptyHandler(category);
        }

        internal override Result PostProcess(Ref visitedType, Result result)
        {
            if(this == visitedType)
                return result;
            return base.PostProcess(visitedType, result);
        }

        public override Result Dereference(Result result)
        {
            return CreateDereferencedArgResult(result.Complete).UseWithArg(result);
        }

        internal override Result DumpPrint(Category category)
        {
            return Target.DumpPrintFromRef(category, RefAlignParam);
        }

        public override Result ApplyTypeOperator(Result argResult)
        {
            return Target.ApplyTypeOperator(argResult);
        }

        public override Result TypeOperator(Category category)
        {
            return Target.TypeOperator(category);
        }

        internal override Result ConvertToVirt(Category category, TypeBase dest)
        {
            return Target
                .ConvertTo(category, dest)
                .UseWithArg(CreateDereferencedArgResult(category));
        }

        internal Result CreateDereferencedArgResult(Category category)
        {
            return Target.CreateResult
                (
                category,
                CreateDereferencedArgCode
                );
        }

        internal CodeBase CreateDereferencedArgCode()
        {
            return CodeBase.CreateArg(Size).CreateDereference(RefAlignParam, Target.Size);
        }

        internal override bool IsConvertableToVirt(TypeBase dest, ConversionFeature conversionFeature)
        {
            return Target.IsConvertableTo(dest, conversionFeature);
        }

        internal protected override SearchResult<IFeature> Search(Defineable defineable)
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

        internal Result AssignmentOperator(Result value)
        {
            var convertedValue = value.ConvertTo(Target);
            var category = value.Complete;
            var result = CreateVoid
                .CreateResult
                (
                category,
                () => CodeBase
                    .CreateArg(Size)
                    .CreateAssign
                    (
                    RefAlignParam,
                    convertedValue.Code
                    ),
                () => convertedValue.Refs
                );
            if(Target.DestructorHandler(category).IsEmpty && Target.MoveHandler(category).IsEmpty)
                return result;

            NotImplementedMethod(value, "result", result);
            throw new NotImplementedException();
        }

        internal Result VisitNextChainElement(ContextBase callContext, Category category, MemberElem memberElem)
        {
            var resultFromRef = SearchDefineable(memberElem.DefineableToken);
            if(resultFromRef.IsSuccessFull)
                return resultFromRef
                    .Feature
                    .VisitApply(callContext, category, memberElem.Args, this);

            NotImplementedMethod(callContext, category, memberElem, "resultFromRef", resultFromRef);
            return null;
        }

        public AssignmentOperatorFeature AssignmentOperatorFeatureObject()
        {
            return _assignmentOperatorFeatureObject;
        }

        internal class AssignmentOperatorFeature : IFeature
        {
            private readonly Ref _ref;

            public AssignmentOperatorFeature(Ref @ref)
            {
                _ref = @ref;
            }

            public Result VisitApply(ContextBase callContext, Category category, SyntaxBase args, Ref callObject)
            {
                Tracer.Assert(callObject == _ref);
                if(category.HasCode || category.HasRefs)
                    return _ref.AssignmentOperator(args.Visit(callContext, category | Category.Type));
                return CreateVoid.CreateResult(category);
            }
        }
    }
}