using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Type
{
    [Serializable]
    internal sealed class AssignableRef : Ref
    {
        internal readonly AssignmentFeature AssignmentFeature;

        internal AssignableRef(TypeBase target, RefAlignParam refAlignParam)
            : base(target, refAlignParam) { AssignmentFeature = new AssignmentFeature(this); }

        protected override string ShortName { get { return "assignable_ref"; } }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            var assignableResult = defineable.SearchFromAssignableRef().SubTrial(this);
            var result = assignableResult.SearchResultDescriptor.Convert(assignableResult.Feature,
                this);
            if(result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
        }
    }

    [Serializable]
    internal class AssignmentFeature : ReniObject, IFeature
    {
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef) { _assignableRef = assignableRef; }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(!category.HasCode && !category.HasRefs && !category.HasInternal)
                return TypeBase.CreateVoid.CreateResult(category);

            var convertTo = callContext.ResultAsRef(category, args, () => callContext.Size(@object).ByteAlignedSize)
                .ConvertTo(_assignableRef.Target);
            var valueResult = convertTo
                .EnsureRef(category, _assignableRef.RefAlignParam,
                    () => callContext.Size(@object).ByteAlignedSize);
            if(valueResult.IsPending)
                return Result.CreatePending(category);
            var destination = callContext.ResultAsRef(category, @object,
                () => valueResult.Internal.Size);
            if(destination.IsPending)
                return Result.CreatePending(category);

            Tracer.Assert(!destination.HasType || destination.Type == _assignableRef);
            var result = TypeBase.CreateVoid
                .CreateResult
                (
                category,
                () =>
                    destination.Code.CreateAssignment(_assignableRef.RefAlignParam, valueResult.Code,
                        _assignableRef.Target.Size),
                () => destination.Refs.Pair(valueResult.Refs),
                () => destination.Internal.CreateSequence(valueResult.Internal)
                );
            if(_assignableRef.Target.DestructorHandler(valueResult.Complete).IsEmpty &&
                _assignableRef.Target.MoveHandler(valueResult.Complete).IsEmpty)
                return (result);

            NotImplementedMethod(callContext, category, @object, args);
            throw new NotImplementedException();
        }
    }
}