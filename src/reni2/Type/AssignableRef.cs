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
        [DumpData(false)]
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
        [DumpData(true)]
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef) { _assignableRef = assignableRef; }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(!category.HasCode && !category.HasRefs && !category.HasInternal)
                return TypeBase.CreateVoid.CreateResult(category);

            var assignmentFeatureResult = callContext.Type(@object).AssignmentFeatureResult(category);

            NotImplementedMethod(callContext, category, @object, args, "assignmentFeatureResult", assignmentFeatureResult);
            throw new NotImplementedException();
        }
    }
}