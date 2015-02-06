using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ArrayReferenceType
        : TypeBase
            , ISymbolProviderForPointer<Mutable, IFeatureImplementation>
            , ISymbolProviderForPointer<EnableReinterpretation, IFeatureImplementation>
            , ISymbolProviderForPointer<TokenClasses.ArrayAccess, IFeatureImplementation>
            , IForcedConversionProvider<ArrayReferenceType>
        , IRepeaterType
    {
        internal sealed class Options : DumpableObject
        {
            OptionsData OptionsData { get; }

            Options(string optionsId)
            {
                OptionsData = new OptionsData(optionsId);
                IsForceMutable = new OptionsData.Option(OptionsData, "force_mutable");
                IsMutable = new OptionsData.Option(OptionsData, "mutable");
                IsEnableReinterpretation = new OptionsData.Option(OptionsData, "enable_reinterpretation");
                OptionsData.Align();
                Tracer.Assert(OptionsData.IsValid);
            }

            internal OptionsData.Option IsMutable { get; }
            internal OptionsData.Option IsForceMutable { get; }
            internal OptionsData.Option IsEnableReinterpretation { get; }

            internal static Options Create(string optionsId) => new Options(optionsId);
            internal static string ForceMutable(bool value) => Create(null).IsForceMutable.SetTo(value);
            protected override string GetNodeDump() => OptionsData.DumpPrintText;
        }

        [Node]
        [SmartNode]
        readonly ValueCache<RepeaterAccessType> _repeaterAccessTypeCache;

        internal ArrayReferenceType(TypeBase valueType, string optionsId)
        {
            options = Options.Create(optionsId);
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);
            _repeaterAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));

            StopByObjectId(-10);
        }

        TypeBase ValueType { get; }
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
        protected override Size GetSize() => ValueType.Pointer.Size;

        [DisableDump]
        internal ArrayReferenceType Mutable => ValueType.ArrayReference(options.IsMutable.SetTo(true));
        [DisableDump]
        internal ArrayReferenceType EnableReinterpretation => ValueType.ArrayReference(options.IsEnableReinterpretation.SetTo(true));

        TypeBase IRepeaterType.ElementType => ValueType;
        Size IRepeaterType.IndexSize => Size;
        bool IRepeaterType.IsMutable => options.IsForceMutable.Value;

        IEnumerable<ISimpleFeature> IForcedConversionProvider<ArrayReferenceType>.Result(ArrayReferenceType destination)
            => ForcedConversion(destination).NullableToArray();

        IFeatureImplementation ISymbolProviderForPointer<Mutable, IFeatureImplementation>.Feature(Mutable tokenClass)
            => Feature.Extension.SimpleFeature(MutableResult);

        IFeatureImplementation ISymbolProviderForPointer<EnableReinterpretation, IFeatureImplementation>.Feature
            (EnableReinterpretation tokenClass)
            => Feature.Extension.SimpleFeature(EnableReinterpretationResult);

        IFeatureImplementation ISymbolProviderForPointer<TokenClasses.ArrayAccess, IFeatureImplementation>.Feature(TokenClasses.ArrayAccess tokenClass)
            => Feature.Extension.FunctionFeature(AccessResult);

        Result MutableResult(Category category)
        {
            Tracer.Assert(options.IsForceMutable.Value);
            return ResultFromPointer(category, Mutable);
        }

        Result EnableReinterpretationResult(Category category) => ResultFromPointer(category, EnableReinterpretation);

        ISimpleFeature ForcedConversion(ArrayReferenceType destination)
        {
            if(this == destination)
                return Feature.Extension.SimpleFeature(ArgResult);

            if(destination.options.IsMutable.Value && !options.IsMutable.Value)
                return null;

            if(ValueType == destination.ValueType)
            {
                NotImplementedMethod(destination);
                return null;
            }

            if(ValueType == destination.ValueType)
            {
                NotImplementedMethod(destination);
                return null;
            }

            if(!options.IsEnableReinterpretation.Value)
                return null;

            return Feature.Extension.SimpleFeature(category => destination.ConversionResult(category, this), this);
        }

        Result ConversionResult(Category category, ArrayReferenceType source)
            => Result(category, () => ConversionCode(source), CodeArgs.Arg);

        CodeBase ConversionCode(ArrayReferenceType source)
        {
            NotImplementedMethod(source);
            return null;
        }

        Result AccessResult(Category category, TypeBase right)
        {
            var indexType = ConversionService.FindPathDestination<NumberType>(right);

            var argsResult = right
                .Conversion(category.Typed, indexType)
                .DereferencedAlignedResult();

            var result = _repeaterAccessTypeCache
                .Value
                .Result(category, PointerObjectResult(category.Typed) + argsResult);

            return result;
        }
    }
}