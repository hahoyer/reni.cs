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
            , ISimpleFeature
            , IReference
            , IFeatureInheritor
    {
        readonly int _order;

        internal PointerType(TypeBase valueType)
        {
            _order = CodeArgs.NextOrder++;
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        [DisableDump]
        TypeBase ValueType { get; }

        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;
        ISimpleFeature IReference.Converter => this;
        bool IReference.IsWeak => true;

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;

        ISimpleFeature IProxyType.Converter => this;
        TypeBase ISimpleFeature.TargetType => ValueType;
        Result ISimpleFeature.Result(Category category) => DereferenceResult(category);
        Result IFeatureInheritor.ConvertToBaseType(Category category) => ArgResult(category);
        TypeBase IFeatureInheritor.BaseType => ValueType;

        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")~~~";


        [DisableDump]
        internal override CompoundView FindRecentCompoundView => ValueType.FindRecentCompoundView;

        [DisableDump]
        internal override IFeatureImplementation Feature => ValueType.Feature;

        [DisableDump]
        internal override bool Hllw => false;

        protected override IEnumerable<ISimpleFeature> ObtainRawSymmetricConversions()
            => base.ObtainRawSymmetricConversions().Concat(new ISimpleFeature[] {Extension.SimpleFeature(DereferenceResult)});

        [DisableDump]
        internal override bool IsAligningPossible => false;

        protected override string GetNodeDump() => ValueType.NodeDump + "[Pointer]";
        internal override int? SmartSequenceLength(TypeBase elementType) => ValueType.SmartSequenceLength(elementType);
        internal override int? SmartArrayLength(TypeBase elementType) => ValueType.SmartArrayLength(elementType);
        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override ArrayType ObtainArray(int count) => ValueType.Array(count);

        Result DereferenceResult(Category category)
        {
            return ValueType
                .Align
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