using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ReferenceType
        : TypeBase
            , ISymbolProviderForPointer<Mutable, IFeatureImplementation>
            , ISymbolProviderForPointer<EnableReinterpretation, IFeatureImplementation>
            , ISymbolProviderForPointer<Target, IFeatureImplementation>
            , IForcedConversionProvider<ReferenceType>
    {
        readonly int _order;

        internal sealed class Options : DumpableObject
        {
            OptionsData OptionsData { get; }

            Options(string optionsId)
            {
                OptionsData = new OptionsData(optionsId);
                IsMutable = new OptionsData.Option(OptionsData,"mutable");
                IsOverSizeable = new OptionsData.Option(OptionsData, "oversizeable");
                IsEnableReinterpretation = new OptionsData.Option(OptionsData,"enable_reinterpretation");
                OptionsData.Align();
                Tracer.Assert(OptionsData.IsValid);
            }

            internal OptionsData.Option IsMutable { get; }
            internal OptionsData.Option IsOverSizeable { get; }
            internal OptionsData.Option IsEnableReinterpretation { get; }

            internal static Options Create(string optionsId = null) => new Options(optionsId);
            internal static readonly string DefaultOptionsId = Create().OptionsData.Id;
            protected override string GetNodeDump()
                => (IsMutable.Value ? "m" : "")
                    + (IsOverSizeable.Value ? "o" : "")
                    + (IsEnableReinterpretation.Value ? "r" : "");
        }

        internal ReferenceType(ArrayType valueType, string optionsId)
        {
            options = Options.Create(optionsId);
            _order = CodeArgs.NextOrder++;
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        ArrayType ValueType { get; }
        Options options { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")reference";
        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> Genericize => this.GenericListFromType(base.Genericize);
        [DisableDump]
        protected override IEnumerable<ISimpleFeature> RawSymmetricConversions => base.RawSymmetricConversions;

        protected override string GetNodeDump() => ValueType.NodeDump + "[reference]" + options.NodeDump;
        protected override Size GetSize() => ValueType.Pointer.Size + ValueType.IndexSize;

        [DisableDump]
        internal ReferenceType Mutable => ValueType.Reference(options.IsMutable.SetTo(true));
        [DisableDump]
        internal ReferenceType OverSizeable => ValueType.Reference(options.IsOverSizeable.SetTo(true));
        [DisableDump]
        internal ReferenceType EnableReinterpretation => ValueType.Reference(options.IsEnableReinterpretation.SetTo(true));

        IEnumerable<ISimpleFeature> IForcedConversionProvider<ReferenceType>.Result(ReferenceType destination)
            => ForcedConversion(destination).NullableToArray();

        IFeatureImplementation ISymbolProviderForPointer<Mutable, IFeatureImplementation>.Feature(Mutable tokenClass)
            => Extension.SimpleFeature(MutableResult);

        IFeatureImplementation ISymbolProviderForPointer<EnableReinterpretation, IFeatureImplementation>.Feature
            (EnableReinterpretation tokenClass)
            => Extension.SimpleFeature(EnableReinterpretationResult);

        IFeatureImplementation ISymbolProviderForPointer<Target, IFeatureImplementation>.Feature(Target tokenClass)
            => Extension.SimpleFeature(TargetResult);

        Result MutableResult(Category category)
        {
            Tracer.Assert(ValueType.IsMutable);
            return ResultFromPointer(category, Mutable);
        }

        Result EnableReinterpretationResult(Category category) => ResultFromPointer(category, EnableReinterpretation);

        ISimpleFeature ForcedConversion(ReferenceType destination)
        {
            if(this == destination)
                return Extension.SimpleFeature(ArgResult);

            if(destination.options.IsMutable.Value && !options.IsMutable.Value)
                return null;

            if(ValueType == destination.ValueType)
            {
                NotImplementedMethod(destination);
                return null;
            }

            if(ValueType.ElementType == destination.ValueType.ElementType)
            {
                NotImplementedMethod(destination);
                return null;
            }

            if(!options.IsEnableReinterpretation.Value)
                return null;

            return Extension.SimpleFeature(category => destination.ConversionResult(category, this), this);
        }

        Result ConversionResult(Category category, ReferenceType source)
            => Result(category, () => ConversionCode(source), CodeArgs.Arg);

        CodeBase ConversionCode(ReferenceType source)
        {
            NotImplementedMethod(source);
            return null;
        }

        Result TargetResult(Category category) => ValueType.Pointer.Result(category, TargetCode, CodeArgs.Arg);

        CodeBase TargetCode()
        {
            NotImplementedMethod();
            return null;
        }
    }
}