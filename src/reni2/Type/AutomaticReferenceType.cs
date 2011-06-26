using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AutomaticReferenceType : ReferenceType
    {
        private readonly RefAlignParam _refAlignParam;
        private readonly AssignmentFeature _assignmentFeature;
        private readonly DictionaryEx<IFunctionalFeature, TypeBase> _functionalTypeCache;

        internal AutomaticReferenceType(TypeBase valueType, RefAlignParam refAlignParam)
            : base(valueType)
        {
            Tracer.Assert(!(valueType is AutomaticReferenceType), valueType.Dump);
            _refAlignParam = refAlignParam;
            _assignmentFeature = new AssignmentFeature(this);
            _functionalTypeCache = new DictionaryEx<IFunctionalFeature, TypeBase>(feature => new FunctionalFeatureType<AutomaticReferenceType, IFunctionalFeature>(this, feature));
            StopByObjectId(-2);
        }

        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        protected override bool IsReferenceTo(TypeBase value) { return ValueType == value; }

        [DisableDump]
        internal override RefAlignParam[] ReferenceChain
        {
            get
            {
                var subResult = ValueType.ReferenceChain;
                var result = new RefAlignParam[subResult.Length + 1];
                result[0] = RefAlignParam;
                subResult.CopyTo(result, 1);
                return result;
            }
        }

        internal Result ApplyAssignment(Category category)
        {
            return FunctionalType(_assignmentFeature)
                .ArgResult(category);
        }

        internal TypeBase FunctionalType(IFunctionalFeature functionalFeature) { return _functionalTypeCache.Find(functionalFeature); }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var result = Void
                .Result
                (
                    category,
                    () => CodeBase.Arg(RefAlignParam.RefSize*2).Assignment(RefAlignParam, ValueType.Size)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsType
                .ConvertTo(category | Category.Type, ValueType)
                .LocalReferenceResult(RefAlignParam);
            var destinationResult = ObjectReferenceInCode(category);
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.ReplaceArg(objectAndSourceRefs);
        }

        internal Result ObjectReferenceInCode(Category category)
        {
            var objectRef = ObjectReference(RefAlignParam);
            var destinationResult = Result
                (
                    category,
                    () => CodeBase.ReferenceCode(objectRef).Dereference(RefAlignParam, Size),
                    () => Refs.Create(objectRef)
                );
            return destinationResult;
        }

        internal override Structure GetStructure() { return ValueType.GetStructure(); }

        internal override Result ReferenceInCode(IReferenceInCode target, Category category)
        {
            return Result
                (
                    category,
                    () => CodeBase.ReferenceCode(target).Dereference(target.RefAlignParam, target.RefAlignParam.RefSize),
                    () => Refs.Create(target)
                );
        }
    }
}