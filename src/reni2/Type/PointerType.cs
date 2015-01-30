using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class PointerType
        : TypeBase
            , IProxyType
            , ISimpleFeature
            , IReference
            , ISymbolProvider<RawAddress, IFeatureImplementation>
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

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")~~~";
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => ValueType.FindRecentCompoundView;
        [DisableDump]
        internal override IFeatureImplementation CheckedFeature => ValueType.CheckedFeature;
        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;

        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;
        ISimpleFeature IReference.Converter => this;
        bool IReference.IsWeak => true;
        ISimpleFeature IProxyType.Converter => this;
        TypeBase ISimpleFeature.TargetType => ValueType;
        Result ISimpleFeature.Result(Category category) => DereferenceResult(category);

        IFeatureImplementation ISymbolProvider<RawAddress, IFeatureImplementation>.Feature(RawAddress tokenClass)
            => Extension.SimpleFeature(RawAddressResult);


        protected override IEnumerable<ISimpleFeature> ObtainRawSymmetricConversions()
            => base.ObtainRawSymmetricConversions().Concat(new ISimpleFeature[] {Extension.SimpleFeature(DereferenceResult)});

        protected override string GetNodeDump() => ValueType.NodeDump + "[Pointer]";
        internal override int? SmartArrayLength(TypeBase elementType) => ValueType.SmartArrayLength(elementType);
        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;
        protected override ArrayType ObtainArray(int count, bool isMutable) => ValueType.Array(count, isMutable);

        internal override IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass)
        {
            var feature = (ValueType as ISymbolProviderForPointer<TDefinable, IFeatureImplementation>)
                ?.Feature(tokenClass);
            var result = feature.NullableToArray().Select(f => new SearchResult(f, Pointer));
            if(result.Any())
                return result;
            return base.Declarations(tokenClass);
        }

        Result DereferenceResult(Category category)
            => ValueType
                .Align
                .Result
                (
                    category,
                    () => ArgCode.DePointer(ValueType.Size).Align(),
                    CodeArgs.Arg
                );

        internal override ResultCache DePointer(Category category)
            => ValueType
                .Result
                (
                    category,
                    () => ArgCode.DePointer(ValueType.Size),
                    CodeArgs.Arg
                );

        Result RawAddressResult(Category category)
        {
            return RootContext
                .BitType
                .RawPointer
                .Result(category, () => ArgCode);
        }
    }
}