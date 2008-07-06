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
            AssignmentFeature = new AssignmentFeature(this);
        }

        protected override string ShortName { get { return "assignable_ref"; } }

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