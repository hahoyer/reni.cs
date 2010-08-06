using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Reference : TypeBase
    {
        [DumpData(true)]
        private readonly Context _context;
        [DumpData(true)]
        private readonly int _position;
        private readonly SimpleCache<TypeBase> _targetCache;

        [DumpData(false)]
        internal readonly AssignmentFeature AssignmentFeature;

        public Reference(Context context, int position)
        {
            _context = context;
            _position = position;
            _targetCache = new SimpleCache<TypeBase>(GetTargetType);
            AssignmentFeature = new AssignmentFeature(this);
        }

        internal Result CreateDumpPrintResult(Category category)
        {
            var refType = CreateTypeReference();
            var result = refType.GenericDumpPrint(category);
            if(result.HasArg)
               result = result.ReplaceArg(CreateAccessResult(category));
            return result;
        }

        private TypeBase Target { get { return _targetCache.Value; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Target.Search(searchVisitor.Child(this));
            Target.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        internal override Result AutomaticDereference(Result result)
        {
            return CreateDereferencedResult(result.CompleteCategory).ReplaceArg(result);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            var destAsRef = AsTypeReference(dest);
            if(destAsRef != null)
                return CreateTypeReference().CreateResult(category,CreateAccessCode);

            return Target
                .Conversion(category, dest)
                .ReplaceArg(CreateDereferencedResult(category));
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

        private RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }

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
                .CreateRefPlus(RefAlignParam, GetOffset(), "CreateAccessCode");
        }

        private Result CreateTargetReferenceResult(Category category)
        {
            return CreateObjectRefInCode(category|Category.Type, RefAlignParam)
                .ConvertTo(CreateTypeReference()) & category;
        }

        private Reni.Type.Reference CreateTypeReference() { return Target.CreateReference(RefAlignParam); }

        private Result CreateDereferencedResult(Category category)
        {
            return Target
                .CreateDereferencedResult(category, RefAlignParam)
                .ReplaceArg(CreateAccessResult(category));
        }

        private Size GetOffset() { return _context.Offset(_position); }
        private TypeBase GetTargetType() { return _context.RawType(_position); }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var result = CreateVoid
                .CreateResult
                (
                    category,
                    () => CodeBase.CreateArg(RefAlignParam.RefSize * 2).CreateAssignment(RefAlignParam, Target.Size)
                );

            if (!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsType.ConvertToAsRef(category, Target.CreateReference(RefAlignParam));
            var destinationResult = CreateTargetReferenceResult(category);
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.ReplaceArg(objectAndSourceRefs);
        }

    }
}