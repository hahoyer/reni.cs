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

        internal override Result AutomaticDereference(Result result) { return CreateAccessResult(result.CompleteCategory).ReplaceArg(result); }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            return Target
                .Conversion(category, dest)
                .ReplaceArg(CreateAccessResult(category));
        }

        protected override Size GetSize() { return Target.Size; }
        internal override string DumpShort() { return String.Format("type(this at {0})", _position); }
        internal override string DumpPrintText { get { return _context.DumpShort() + " AT " + _position; } }

        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature) { return Target.IsConvertableTo(dest, conversionFeature); }

        internal override IFunctionalFeature GetFunctionalFeature() { return Target.GetFunctionalFeature(); }

        internal override int GetSequenceCount(TypeBase elementType) { return Target.GetSequenceCount(elementType); }
        internal override TypeBase GetTypeForTypeOperator() { return Target.GetTypeForTypeOperator(); }

        private Result CreateAccessResult(Category category) { return CreateResult(category, CreateAccessCode); }

        private CodeBase CreateAccessCode()
        {
            return CodeBase.CreateArg(RefAlignParam.RefSize)
                .CreateRefPlus(RefAlignParam, GetOffset(), "CreateAccessCode")
                .CreateDereference(RefAlignParam, Size);
        }

        private Result CreateTargetReferenceResult(Category category)
        {
            return CreateObjectRefInCode(category | Category.Type, RefAlignParam)
                       .ConvertTo(CreateTypeReference()) & category;
        }

        internal Reni.Type.Reference CreateTypeReference() { return Target.CreateReference(RefAlignParam); }

        private RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        private Size GetOffset() { return _context.Offset(_position); }
        private TypeBase GetTargetType() { return _context.RawType(_position); }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var result = CreateVoid
                .CreateResult
                (
                    category,
                    () => CodeBase.CreateArg(RefAlignParam.RefSize*2).CreateAssignment(RefAlignParam, Target.Size)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsType.ConvertTo(category | Category.Type, Target).CreateLocalReferenceResult(RefAlignParam);
            var destinationResult = CreateTargetReferenceResult(category);
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.ReplaceArg(objectAndSourceRefs);
        }

        internal Result ApplyAssignment(Category category)
        {
            return CreateTypeReference()
                .CreateFunctionalType(AssignmentFeature)
                .CreateArgResult(category);
        }
    }
}