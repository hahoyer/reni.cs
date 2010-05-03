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
        internal readonly IFeature DumpPrintFeature;

        public Reference(Context context, int position)
        {
            _context = context;
            _position = position;
            _targetCache = new SimpleCache<TypeBase>(GetTargetType);
            DumpPrintFeature = new Feature.DumpPrint.Feature<Reference>(this);
        }

        protected override Result ConvertTo_Implementation(Category category, TypeBase dest)
        {
            var result1 = _targetCache.Value.Conversion(category, dest);
            var accessResult = CreateAccessResult(category);
            var dereference = _targetCache.Value.DereferencedResult(category, accessResult, RefAlignParam);
            var result = result1.UseWithArg(dereference);
            return result;
        }

        protected override Size GetSize() { return _context.RefSize; }
        internal override string DumpShort() { return String.Format("type(this at {0})", _position); }
        internal override string DumpPrintText { get { return _context.DumpShort() + " AT " + _position; } }
        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature) { return _targetCache.Value.IsConvertableTo(dest, conversionFeature); }

        internal override bool IsRef(RefAlignParam refAlignParam)
        {
            Tracer.Assert(RefAlignParam == refAlignParam);
            return true;
        }

        internal override int GetSequenceCount(TypeBase elementType) { return _targetCache.Value.GetSequenceCount(elementType); }
        internal override TypeBase GetEffectiveType() { return _targetCache.Value.GetEffectiveType(); }

        private Result CreateAccessResult(Category category) { return CreateResult(category, CreateAccessCode); }

        private CodeBase CreateAccessCode() { return CreateArgCode().CreateRefPlus(RefAlignParam, GetOffset()); }

        internal Size TargetSize { get { return _targetCache.Value.Size; } }

        private Size GetOffset() { return _context.Offset(_position); }
        private TypeBase GetTargetType() { return _context.InternalType(_position); }
        private RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }

        Result IFeatureTarget.Apply(Category category)
        {
            return _targetCache
                .Value
                .DumpPrintFromReference(category, CreateAccessResult(category), RefAlignParam);
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.Child(this).Search();
            base.Search(searchVisitor);
        }

    }
}

