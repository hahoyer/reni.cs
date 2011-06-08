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
        [IsDumpEnabled]
        private readonly PositionContainerContext _context;

        private readonly SimpleCache<TypeBase> _valueTypeCache;

        internal Field(PositionContainerContext context)
        {
            _context = context;
            _valueTypeCache = new SimpleCache<TypeBase>(GetValueType);
        }

        private int Position { get { return _context.Position; } }

        internal Result DumpPrintResult(Category category)
        {
            var refType = ValueTypeReference;
            var result = refType.GenericDumpPrint(category);
            if(result.HasArg)
                result = result.ReplaceArg(AccessResult(category));
            return result;
        }

        private TypeBase ValueType { get { return _valueTypeCache.Value; } }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor.Child(this));
            ValueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

        protected override Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, dest);
            var resultForArg = AccessResult(category).ReplaceArg(LocalReference(category));
            return ReturnMethodDumpWithBreak(trace, ValueType
                                                        .Conversion(category, dest)
                                                        .ReplaceArg(resultForArg));
        }

        private Result LocalReference(Category category)
        {
            return ArgResult(category | Category.Type)
                .LocalReferenceResult(RefAlignParam);
        }

        protected override TypeBase Dereference() { return ValueType; }
        protected override Result DereferenceResult(Category category) { return AccessResult(category); }
        protected override Size GetSize() { return ValueType.Size; }
        internal override string DumpShort() { return String.Format("type(this at {0})", Position); }

        internal override string DumpPrintText { get { return _context.DumpShort() + " AT " + Position; } }
        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return ValueType.IsConvertableTo(dest, conversionParameter); }
        internal override IFunctionalFeature FunctionalFeature() { return ValueType.FunctionalFeature(); }
        internal override int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal override TypeBase TypeForTypeOperator() { return ValueType.TypeForTypeOperator(); }

        private Result AccessResult(Category category)
        {
            return ValueTypeReference
                .Result(category, GetAccessCode);
        }

        private RefAlignParam RefAlignParam { get { return _context.RefAlignParam; } }
        private Size Offset { get { return _context.InnerOffset; } }
        private Reference ValueTypeReference { get { return ValueType.Reference(RefAlignParam); } }

        private TypeBase GetValueType() { return _context.InnerType; }

        private CodeBase GetAccessCode()
        {
            return _context
                .ContextReferenceType
                .LocalReferenceCode()
                .AddToReference(RefAlignParam, Offset, "GetAccessCode");
        }

        Result IAccessType.Result(Category category) { return AccessResult(category); }
    }
}