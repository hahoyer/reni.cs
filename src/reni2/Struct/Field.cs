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
    internal sealed class Field : ReniObject, IAccessObject, IDumpShortProvider
    {
        [EnableDump]
        private readonly PositionContainerContext _context;

        private readonly SimpleCache<TypeBase> _valueTypeCache;

        internal Field(PositionContainerContext context)
        {
            _context = context;
            _valueTypeCache = new SimpleCache<TypeBase>(GetValueType);
        }

        Result IAccessObject.Result(Category category) { return AccessResult(category); }
        string IDumpShortProvider.DumpShort() { return String.Format("type(this at {0})", Position); }

        private PositionContainerContext Context { get { return _context; } }
        private int Position { get { return Context.Position; } }
        private TypeBase ValueType { get { return _valueTypeCache.Value; } }
        internal Reference ValueTypeReference { get { return ValueType.Reference(RefAlignParam); } }
        private RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }
        private Size Offset { get { return Context.StructSize; } }

        internal void Search(ISearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor.Child(this));
            ValueType.Search(searchVisitor);
        }

        protected Result ConvertToImplementation(Category category, TypeBase dest)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, dest);
            var resultForArg = AccessResult(category).ReplaceArg(LocalReference(category));
            var result = ValueType
                .Conversion(category, dest)
                .ReplaceArg(resultForArg);
            return ReturnMethodDumpWithBreak(trace, result);
        }

        protected TypeBase Dereference() { return ValueType; }
        protected Result DereferenceResult(Category category) { return AccessResult(category); }
        protected Size GetSize() { return RefAlignParam.RefSize; }
        internal string DumpPrintText { get { return Context.DumpShort() + " AT " + Position; } }

        internal bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return ValueType.IsConvertableTo(dest, conversionParameter); }
        internal IFunctionalFeature FunctionalFeature() { return ValueType.FunctionalFeature(); }
        internal int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal TypeBase TypeForTypeOperator() { return ValueType.TypeForTypeOperator(); }

        internal Result DumpPrintResult(Category category)
        {
            var refType = ValueTypeReference;
            var result = refType.GenericDumpPrint(category);
            if (result.HasArg)
                result = result.ReplaceArg(AccessResult(category));
            return result;
        }

        private Result LocalReference(Category category)
        {
            return ValueType.ArgResult(category | Category.Type)
                .LocalReferenceResult(RefAlignParam);
        }

        private Result AccessResult(Category category)
        {
            return ValueTypeReference
                .Result(category, GetAccessCode);
        }

        private TypeBase GetValueType() { return Context.InnerType; }

        private CodeBase GetAccessCode()
        {
            return Context
                .ContextType
                .LocalCode()
                .AddToReference(RefAlignParam, Offset, "GetAccessCode");
        }

    }
}