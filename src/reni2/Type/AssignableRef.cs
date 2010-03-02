using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    internal sealed class StructRef : TypeBase
    {
        private readonly Struct.Context _context;
        private readonly int _position;
        private readonly SimpleCache<TypeBase> _targetCache;

        [DumpData(false)]
        internal readonly AssignmentFeature AssignmentFeature;

        [DumpData(false)]
        internal readonly DumpPrintFeature DumpPrintFeature;

        public StructRef(Struct.Context context, int position)
        {
            _context = context;
            _position = position;
            _targetCache = new SimpleCache<TypeBase>(GetTargetType);
            AssignmentFeature = new AssignmentFeature(this);
            DumpPrintFeature = new DumpPrintFeature(this);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest) { return _targetCache.Value.GetResult(RefAlignParam, category, dest); }
        protected override Size GetSize() { return _context.RefSize; }
        internal override string DumpShort() { return String.Format("type(this at {0})", _position); }
        internal override string DumpPrintText { get { return _context.DumpShort() + " AT " + _position; } }
        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature) { return _targetCache.Value.IsConvertableTo(dest, conversionFeature); }
        internal override bool IsValidRefTarget() { return false; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            _targetCache.Value.ChildSearch(searchVisitor, this);
            base.Search(searchVisitor);
        }

        internal override int SequenceCount { get { return 1; } }

        internal AutomaticRef CreateAutomaticRef() { return _targetCache.Value.CreateAutomaticRef(_context.RefAlignParam); }

        internal Result DumpPrint(Category category)
        {
            return _targetCache.Value
                .DumpPrintFromReference(category, CreateAccessResult(category), RefAlignParam);
        }

        private Result CreateAccessResult(Category category) { return CreateResult(category, CreateAccessCode); }

        private CodeBase CreateAccessCode()
        {
            return CreateArgCode()
                .CreateRefPlus(RefAlignParam, GetOffset());
        }

        internal RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        internal Size TargetSize { get { return _targetCache.Value.Size; } }

        private Size GetOffset() { return _context.Offset(_position); }
        private TypeBase GetTargetType() { return _context.InternalType(_position); }
    }

    internal sealed class DumpPrintFeature : ReniObject, IFeature
    {
        private readonly StructRef _structRef;

        public DumpPrintFeature(StructRef structRef) { _structRef = structRef; }

        Result IFeature.Apply(Category category) { return _structRef.DumpPrint(category); }

        TypeBase IFeature.DefiningType() { return _structRef; }
    }

    internal sealed class AssignmentFeature : ReniObject, IFeature, IFunctionalFeature
    {
        [DumpData(true)]
        private readonly StructRef _structRef;

        public AssignmentFeature(StructRef structRef) { _structRef = structRef; }

        Result IFeature.Apply(Category category) { return _structRef.CreateFunctionalType(this).CreateArgResult(category); }

        Result IFunctionalFeature.Apply(Category category, Result functionalResult, Result argsResult)
        {
            var result = TypeBase
                .CreateVoid
                .CreateResult
                (
                    category,
                    () =>
                    CodeBase
                        .CreateArg(_structRef.Size*2)
                        .CreateAssignment(_structRef.RefAlignParam, _structRef.TargetSize)
                );

            if(!category.HasCode && !category.HasRefs)
                return result;

            var sourceResult = argsResult.ConvertToAsRef(category, _structRef.CreateAutomaticRef());
            var destinationResult = functionalResult.StripFunctional() & category;
            var objectAndSourceRefs = destinationResult.CreateSequence(sourceResult);
            return result.UseWithArg(objectAndSourceRefs);
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        string IDumpShortProvider.DumpShort() { return _structRef.DumpShort() + " :="; }
    }
}