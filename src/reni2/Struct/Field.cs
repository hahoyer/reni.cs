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
    internal sealed class Field : TypeBase, IAccessType
    {
        [IsDumpEnabled(true)]
        private readonly Context _context;
        [IsDumpEnabled(true)]
        private readonly int _position;
        private readonly SimpleCache<TypeBase> _targetCache;
        private readonly AssignmentFeature _assignmentFeature;

        public Field(Context context, int position)
        {
            _context = context;
            _position = position;
            _targetCache = new SimpleCache<TypeBase>(GetTargetType);
            _assignmentFeature = new AssignmentFeature(this);
        }

        internal Result DumpPrintResult(Category category)
        {
            var refType = TypeReference();
            var result = refType.GenericDumpPrint(category);
            if(result.HasArg)
                result = result.ReplaceArg(AccessResult(category));
            return result;
        }

        private TypeBase Target { get { return _targetCache.Value; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            Target.Search(searchVisitor.Child(this));
            Target.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, dest);
            var resultForArg = AccessResult(category).ReplaceArg(LocalReference(category));
            return ReturnMethodDumpWithBreak(trace, Target
                                                        .Conversion(category, dest)
                                                        .ReplaceArg(resultForArg));
        }

        private Result LocalReference(Category category)
        {
            return ArgResult(category|Category.Type)
                .LocalReferenceResult(RefAlignParam);
        }

        protected override TypeBase Dereference() { return Target; }
        protected override Result DereferenceResult(Category category) { return AccessResult(category); }
        protected override Size GetSize() { return Target.Size; }
        internal override string DumpShort() { return String.Format("type(this at {0})", _position); }
        internal override string DumpPrintText { get { return _context.DumpShort() + " AT " + _position; } }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return Target.IsConvertableTo(dest, conversionParameter); }
        internal override IFunctionalFeature FunctionalFeature() { return Target.FunctionalFeature(); }
        internal override int SequenceCount(TypeBase elementType) { return Target.SequenceCount(elementType); }
        internal override TypeBase TypeForTypeOperator() { return Target.TypeForTypeOperator(); }

        internal Result ApplyAssignment(Category category, TypeBase argsType)
        {
            var result = Void
                .Result
                (
                    category,
                    () => CodeBase.Arg(RefAlignParam.RefSize * 2).CreateAssignment(RefAlignParam, Target.Size)
                );

            if (!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsType.ConvertTo(category | Category.Type, Target).LocalReferenceResult(RefAlignParam);
            var destinationResult = TargetReferenceResult(category);
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.ReplaceArg(objectAndSourceRefs);
        }

        internal Result ApplyAssignment(Category category)
        {
            return TypeReference()
                .FunctionalType(_assignmentFeature)
                .ArgResult(category);
        }

        private Result AccessResult(Category category) { return Target.Result(category, AccessCode); }
        private RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        private Size GetOffset() { return _context.Offset(_position); }
        private TypeBase GetTargetType() { return _context.RawType(_position); }
        private Reference TypeReference() { return Target.Reference(RefAlignParam); }

        private CodeBase AccessCode()
        {
            return _context.ContextReferenceType.LocalReferenceCode(RefAlignParam)
                .AddToReference(RefAlignParam, GetOffset(), "AccessCode")
                .Dereference(RefAlignParam, Size);
        }

        private Result TargetReferenceResult(Category category)
        {
            return ObjectRefInCode(category | Category.Type, RefAlignParam)
                       .ConvertTo(TypeReference()) & category;
        }

        Result IAccessType.Result(Category category) { return AccessResult(category); }
    }
}