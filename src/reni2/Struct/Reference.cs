using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Reference : TypeBase, IFeatureTarget
    {
        private readonly Context _context;
        private readonly int _position;
        private readonly SimpleCache<TypeBase> _targetCache;

        [DumpData(false)]
        internal readonly IFeature DumpPrintFeature;
        [DumpData(false)]
        internal readonly AssignmentFeature AssignmentFeature;

        public Reference(Context context, int position)
        {
            _context = context;
            _position = position;
            _targetCache = new SimpleCache<TypeBase>(GetTargetType);
            DumpPrintFeature = new Feature<Reference>(this);
            AssignmentFeature = new AssignmentFeature(this);
        }

        Result IFeatureTarget.Apply(Category category)
        {
            return Target
                .DumpPrintFromReference(category, CreateAccessResult(category), RefAlignParam);
        }

        internal TypeBase Target { get { return _targetCache.Value; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Target.Search(searchVisitor.Child(this));
            Target.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            var destAsRef = AsTypeReference(dest);
            if(destAsRef != null)
                return CreateTypeReferenceResult(category);

            return Target
                .Conversion(category, dest)
                .UseWithArg(CreateDereferencedResult(category));
        }

        protected override Size GetSize() { return _context.RefSize; }
        internal override string DumpShort() { return String.Format("type(this at {0})", _position); }
        internal override string DumpPrintText { get { return _context.DumpShort() + " AT " + _position; } }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature)
        {
            return AsTypeReference(dest) != null
                   || Target.IsConvertableTo(dest, conversionFeature);
        }

        internal override IFunctionalFeature GetFunctionalFeature() { return Target.GetFunctionalFeature(); }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }
        internal override int GetSequenceCount(TypeBase elementType) { return Target.GetSequenceCount(elementType); }
        internal override TypeBase GetEffectiveType() { return Target.GetEffectiveType(); }

        internal Result ApplyAssignment(Category category, Result functionalResult, Result argsResult)
        {
            var result = CreateVoid
                .CreateResult
                (
                    category,
                    () => CodeBase.CreateArg(RefAlignParam.RefSize * 2).CreateAssignment(RefAlignParam, Size)
                );

            if (!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsResult.ConvertToAsRef(category, CreateReference(RefAlignParam));
            var destinationResult = functionalResult.StripFunctional() & category;
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }

        private Reni.Type.Reference AsTypeReference(TypeBase dest)
        {
            var destAsRef = dest as Reni.Type.Reference;
            if(destAsRef == null)
                return null;
            if(RefAlignParam != destAsRef.RefAlignParam)
                return null;
            if(Target != destAsRef.Target)
                return null;

            return destAsRef;
        }

        private Result CreateAccessResult(Category category) { return CreateResult(category, CreateAccessCode); }

        private CodeBase CreateAccessCode()
        {
            return CreateArgCode()
                .CreateRefPlus(RefAlignParam, GetOffset());
        }

        private Result CreateDereferencedResult(Category category)
        {
            var accessResult = CreateAccessResult(category);
            return Target.CreateDereferencedResult(category, accessResult, RefAlignParam);
        }

        private Size GetOffset() { return _context.Offset(_position); }
        private TypeBase GetTargetType() { return _context.InternalType(_position); }
        private RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }

        private Result CreateTypeReferenceResult(Category category)
        {
            return CreateAccessResult(category);
        }

    }
}