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
        internal override AutomaticRef CreateAutomaticRef() { return Target.CreateAutomaticRef(RefAlignParam); }
    }

    [Serializable]
    internal class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature
    {
        [DumpData(true)]
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef) { _assignableRef = assignableRef; }

        Result IFeature.Apply(Category category, TypeBase objectType) { return objectType.CreateFunctionalType(this).CreateArgResult(category); }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var result = TypeBase.CreateVoid.CreateResult(
                category,
                () => CodeBase.CreateArg(_assignableRef.Size * 2).CreateAssignment(_assignableRef.RefAlignParam, _assignableRef.Target.Size)
                );

            if (!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsResult.ConvertToAsRef(category, _assignableRef.CreateAutomaticRef());
            var objectAndSourceRefs = functionalResult.StripFunctional().CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }

        string IDumpShortProvider.DumpShort() { return _assignableRef.DumpShort() + " :="; }

    }
}