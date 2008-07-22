using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;

namespace Reni.Type
{
    internal sealed class AssignableRef : Ref
    {
        internal readonly AssignmentFeature AssignmentFeature;

        internal AssignableRef(TypeBase target, RefAlignParam refAlignParam)
            : base(target, refAlignParam)
        {
            Tracer.Assert(!(target is Aligner) );
            Tracer.Assert(!(target is Void));
            AssignmentFeature = new AssignmentFeature(this);
        }

        protected override string ShortName { get { return "assignable_ref"; } }

        internal override SearchResult<IFeature> Search(Defineable defineable)
        {
            var assignableResult = defineable.SearchFromAssignableRef().SubTrial(this);
            var result = assignableResult.SearchResultDescriptor.Convert(assignableResult.Feature, this);
            if (result.IsSuccessFull)
                return result;
            return base.Search(defineable).AlternativeTrial(result);
        }
    }

    internal class AssignmentFeature : ReniObject, IFeature
    {
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef)
        {
            _assignableRef = assignableRef;
        }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(!category.HasCode && !category.HasRefs && !category.HasInternal)
                return TypeBase.CreateVoid.CreateResult(category);

            return callContext
                .Result(category, @object)
                .CreateAssignment(_assignableRef, callContext.ResultAsRef(category | Category.Type, args));
        }
    }
}