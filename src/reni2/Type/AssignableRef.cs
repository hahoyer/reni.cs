using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

#pragma warning disable 1911

namespace Reni.Type
{
    [Serializable]
    internal sealed class AssignableRef : TypeBase
    {
        private static int _nextObjectId;
        private readonly Struct.Type _target;
        internal readonly RefAlignParam RefAlignParam;
        [DumpData(false)]
        internal readonly AssignmentFeature AssignmentFeature;

        internal AssignableRef(Struct.Type target, RefAlignParam refAlignParam)
            : base(_nextObjectId++)
        {
            _target = target;
            RefAlignParam = refAlignParam;
            AssignmentFeature = new AssignmentFeature(this);
        }

        protected override Size GetSize() { return RefAlignParam.RefSize; }
        
        internal override string DumpShort() { return "ref." + _target.DumpShort(); }
        internal override bool IsValidRefTarget() { return false; }
        internal Size TargetSize { get { return _target.Size; } }
        internal AutomaticRef CreateAutomaticRef() { return _target.CreateAutomaticRef(RefAlignParam); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _target.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

    }

    [Serializable]
    internal class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature
    {
        [DumpData(true)]
        private readonly AssignableRef _assignableRef;

        public AssignmentFeature(AssignableRef assignableRef) { _assignableRef = assignableRef; }

        Result IFeature.Apply(Category category) { return _assignableRef.CreateFunctionalType(this).CreateArgResult(category); }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var result = TypeBase.CreateVoid.CreateResult(
                category,
                () =>
                CodeBase
                .CreateArg(_assignableRef.Size*2)
                .CreateAssignment(_assignableRef.RefAlignParam,_assignableRef.TargetSize)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsResult.ConvertToAsRef(category, _assignableRef.CreateAutomaticRef());
            var destinationResult = functionalResult.StripFunctional() & category;
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        string IDumpShortProvider.DumpShort() { return _assignableRef.DumpShort() + " :="; }
    }
}