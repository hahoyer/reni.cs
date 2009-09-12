using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser.TokenClass;
using Reni.Syntax;

#pragma warning disable 1911

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

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Parent.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }
    }

    [Serializable]
    internal class AssignmentFeature : ReniObject, IFeature
    {
        [DumpData(true)]
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef) { _assignableRef = assignableRef; }

        TypeBase IFeature.ResultType { get { return null; } }

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            NotImplementedMethod(category, objectType);
            return null;
        }

        Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var destinationType = (AssignableRef)callContext.Type(@object);

            var result = TypeBase.CreateVoid.CreateResult(
                category,
                () => CodeBase.CreateArg(destinationType.Size*2).CreateAssignment(callContext.RefAlignParam,destinationType.Target.Size)
                );

            if (!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = callContext.ConvertedRefResult
                (
                category, 
                args, 
                destinationType.Target.CreateAutomaticRef(destinationType.RefAlignParam)
                );
            var objectAndSourceRefs = callContext.Result(category, @object).CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }
    }
}