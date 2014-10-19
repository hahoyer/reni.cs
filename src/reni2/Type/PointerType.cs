using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    sealed class PointerType
        : TypeBase
            , IProxyType
            , IConverter
            , IReferenceType
            , IFeatureInheritor
    {
        readonly TypeBase _valueType;
        readonly int _order;

        internal PointerType(TypeBase valueType)
        {
            _order = CodeArgs.NextOrder++;
            _valueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        Size IContextReference.Size { get { return Size; } }
        int IContextReference.Order { get { return _order; } }
        IConverter IReferenceType.Converter { get { return this; } }
        bool IReferenceType.IsWeak { get { return true; } }

        [DisableDump]
        internal override Root RootContext { get { return _valueType.RootContext; } }

        IConverter IProxyType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return ValueType; } }
        Result IConverter.Result(Category category) { return DereferenceResult(category); }
        Result IFeatureInheritor.Source(Category category) { return DereferenceResult(category); }

        internal override string DumpPrintText { get { return "(" + ValueType.DumpPrintText + ")~~~"; } }

        [DisableDump]
        internal TypeBase ValueType { get { return _valueType; } }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return ValueType.FindRecentStructure; } }

        [DisableDump]
        internal override IFeatureImplementation Feature { get { return ValueType.Feature; } }

        [DisableDump]
        internal override bool Hllw { get { return false; } }

        [DisableDump]
        protected override IEnumerable<ISimpleFeature> Conversions
        {
            get { return base.Conversions.Concat(new ISimpleFeature[] { Extension.SimpleFeature(DereferenceResult) }); }
        }

        [DisableDump]
        internal override bool IsAligningPossible { get { return false; } }
        
        protected override string GetNodeDump() { return ValueType.NodeDump + "[Pointer]"; }
        internal override int? SmartSequenceLength(TypeBase elementType) { return ValueType.SmartSequenceLength(elementType); }
        internal override int? SmartArrayLength(TypeBase elementType) { return ValueType.SmartArrayLength(elementType); }
        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }
        protected override ArrayType ObtainArray(int count) { return ValueType.UniqueArray(count); }

        Result DereferenceResult(Category category)
        {
            return ValueType
                .UniqueAlign
                .Result
                (
                    category,
                    () => ArgCode.DePointer(ValueType.Size).Align(),
                    CodeArgs.Arg
                );
        }

        internal override ResultCache DePointer(Category category)
        {
            return ValueType
                .Result
                (
                    category,
                    () => ArgCode.DePointer(ValueType.Size),
                    CodeArgs.Arg
                );
        }
    }
}