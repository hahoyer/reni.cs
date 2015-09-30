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
            , IConversion
            , IReference
    {
        readonly int _order;

        internal PointerType(TypeBase valueType)
        {
            _order = CodeArgs.NextOrder++;
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(valueType.IsPointerPossible, valueType.Dump);
            StopByObjectIds(-10);
        }

        [DisableDump]
        internal TypeBase ValueType { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")~~~";
        [DisableDump]
        internal override CompoundView FindRecentCompoundView => ValueType.FindRecentCompoundView;
        [DisableDump]
        internal override IImplementation CheckedFeature => ValueType.CheckedFeature;
        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        internal override bool IsPointerPossible => false;

        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;
        IConversion IReference.Converter => this;
        bool IReference.IsWeak => true;
        IConversion IProxyType.Converter => this;
        TypeBase IConversion.Source => this;
        Result IConversion.Execute(Category category) => DereferenceResult(category);

        [DisableDump]
        protected override IEnumerable<IConversion> RawSymmetricConversions
            =>
                base.RawSymmetricConversions.Concat
                    (new IConversion[] {Feature.Extension.Conversion(DereferenceResult)});

        protected override string GetNodeDump() => ValueType.NodeDump + "[Pointer]";

        internal override int? SmartArrayLength(TypeBase elementType)
            => ValueType.SmartArrayLength(elementType);

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

        protected override ArrayType ArrayForCache(int count, string optionsId)
            => ValueType.Array(count, optionsId);

        internal override IEnumerable<IConversion> GetForcedConversions<TDestination>
            (TDestination destination)
        {
            var provider = ValueType as IForcedConversionProviderForPointer<TDestination>;
            if(provider != null)
                foreach(var feature in provider.Result(destination))
                    yield return feature;

            foreach(var feature in base.GetForcedConversions(destination))
                yield return feature;
        }

        [DisableDump]
        internal override IEnumerable<IConversion> StripConversions
            => ValueType.StripConversionsFromPointer;

        [DisableDump]
        internal override IImplementation FuncionDeclarationForType
            => ValueType.FunctionDeclarationForPointerType
                ?? base.FuncionDeclarationForType;

        internal override IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass)
        {
            var feature = (ValueType as ISymbolProviderForPointer<TDefinable>)?.Feature(tokenClass);
            if(feature == null)
                return base.Declarations(tokenClass);

            return new[]
            {
                SearchResult.Create(feature, this)
            };
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

        protected override ResultCache DePointer(Category category)
            => ValueType
                .Result
                (
                    category,
                    () => ArgCode.DePointer(ValueType.Size),
                    CodeArgs.Arg
                );
    }
}