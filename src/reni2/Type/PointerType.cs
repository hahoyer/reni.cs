using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
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
            , IConversion
            , IReference
            , IChild<TypeBase>
            , ISymbolProvider<StableReference>
    {
        internal sealed class Options : DumpableObject
        {
            Flags Data { get; }

            Options(string optionsId)
            {
                Data = new Flags(optionsId);
                IsStable = Data.Register("stable_reference");
                Data.Align();
                Tracer.Assert(Data.IsValid);
            }

            internal Flag IsStable { get; }

            internal static Options Create(string optionsId) => new Options(optionsId);

            internal static string Stable(bool value)
                => Create(null).IsStable.SetTo(value);

            protected override string GetNodeDump() => DumpPrintText;
            public string DumpPrintText => Data.DumpPrintText;
        }

        readonly int _order;

        internal PointerType(TypeBase valueType, string optionsId)
        {
            _order = CodeArgs.NextOrder++;
            OptionsValue = Options.Create(optionsId);
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(valueType.IsPointerPossible, valueType.Dump);
            StopByObjectIds(-10);
        }

        [Node]
        [DisableDump]
        internal TypeBase ValueType { get; }
        Options OptionsValue { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText
            => "(" + ValueType.DumpPrintText + ")" + OptionsValue.DumpPrintText;

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
        [DisableDump]
        internal bool IsStable => OptionsValue.IsStable.Value;
        [DisableDump]
        internal PointerType StableReference
            => ValueType.PointerType(OptionsValue.IsStable.SetTo(true));

        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;
        IConversion IReference.Converter => this;
        bool IReference.IsWeak => !IsStable;
        IConversion IProxyType.Converter => this;
        TypeBase IConversion.Source => this;
        Result IConversion.Execute(Category category) => DereferenceResult(category);
        TypeBase IChild<TypeBase>.Parent => ValueType;

        [DisableDump]
        protected override IEnumerable<IConversion> RawSymmetricConversions
        {
            get
            {
                foreach(var conversion in base.RawSymmetricConversions)
                    yield return conversion;

                if(!IsStable)
                    yield return Feature.Extension.Conversion(DereferenceResult);

                yield return Feature.Extension.Conversion(ToggleStableConversion);
            }
        }

        Result ToggleStableConversion(Category category) => Mutation(ToggleStable) & category;

        [DisableDump]
        TypeBase ToggleStable => ValueType.PointerType(OptionsValue.IsStable.SetTo(!IsStable));

        protected override string GetNodeDump()
            => ValueType.NodeDump + "[Pointer]" + OptionsValue.DumpPrintText;

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

        IImplementation ISymbolProvider<StableReference>.Feature
            (StableReference tokenClass)
            => Feature.Extension.Value(StableReferenceResult);

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

        Result StableReferenceResult(Category category)
            => ResultFromPointer(category, StableReference);
    }
}