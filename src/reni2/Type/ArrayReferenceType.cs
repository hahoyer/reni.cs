using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Numeric;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ArrayReferenceType
        : TypeBase
            , ISymbolProvider<Mutable, IFeatureImplementation>
            , ISymbolProvider<EnableReinterpretation, IFeatureImplementation>
            , ISymbolProvider<TokenClasses.ArrayAccess, IFeatureImplementation>
            , ISymbolProvider<Plus, IFeatureImplementation>
            , ISymbolProvider<Minus, IFeatureImplementation>
            , IForcedConversionProvider<ArrayReferenceType>
            , IRepeaterType
            , IReference
            , ISimpleFeature
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
            protected override string GetNodeDump() => DumpPrintText;
            public string DumpPrintText => OptionsData.DumpPrintText;
        }

        readonly int _order;
        readonly ValueCache<RepeaterAccessType> _repeaterAccessTypeCache;

        internal ArrayReferenceType(TypeBase valueType, string optionsId)
        {
            _order = CodeArgs.NextOrder++;
            options = Options.Create(optionsId);
            _repeaterAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);

            StopByObjectId(-10);
        }

        TypeBase ValueType { get; }
        Options options { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText => "(" + ValueType.DumpPrintText + ")reference" + options.DumpPrintText;
        [DisableDump]
        RepeaterAccessType AccessType => _repeaterAccessTypeCache.Value;
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
        internal ArrayReferenceType EnableReinterpretation
            => ValueType.ArrayReference(options.IsEnableReinterpretation.SetTo(true));

        TypeBase IRepeaterType.ElementType => ValueType;
        TypeBase IRepeaterType.IndexType => RootContext.BitType.Number(Size.ToInt());
        bool IRepeaterType.IsMutable => options.IsForceMutable.Value;

        ISimpleFeature IReference.Converter => this;
        bool IReference.IsWeak => false;
        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;

        TypeBase ISimpleFeature.TargetType => ValueType;
        Result ISimpleFeature.Result(Category category) => DereferenceResult(category);

        IEnumerable<ISimpleFeature> IForcedConversionProvider<ArrayReferenceType>.Result(ArrayReferenceType destination)
            => ForcedConversion(destination).NullableToArray();

        IFeatureImplementation ISymbolProvider<Mutable, IFeatureImplementation>.Feature(Mutable tokenClass)
            => Extension.SimpleFeature(MutableResult);

        IFeatureImplementation ISymbolProvider<EnableReinterpretation, IFeatureImplementation>.Feature
            (EnableReinterpretation tokenClass)
            => Extension.SimpleFeature(EnableReinterpretationResult);

        IFeatureImplementation ISymbolProvider<TokenClasses.ArrayAccess, IFeatureImplementation>.Feature
            (TokenClasses.ArrayAccess tokenClass)
            => Extension.FunctionFeature(AccessResult);

        IFeatureImplementation ISymbolProvider<Minus, IFeatureImplementation>.Feature(Minus tokenClass)
            => Extension.FunctionFeature(MinusResult);

        IFeatureImplementation ISymbolProvider<Plus, IFeatureImplementation>.Feature(Plus tokenClass)
            => Extension.FunctionFeature(PlusResult);


        Result MutableResult(Category category)
        {
            Tracer.Assert(options.IsForceMutable.Value);
            return ResultFromPointer(category, Mutable);
        }

        Result EnableReinterpretationResult(Category category) => ResultFromPointer(category, EnableReinterpretation);

        ISimpleFeature ForcedConversion(ArrayReferenceType destination)
            =>
                HasForcedConversion(destination)
                    ? Extension.SimpleFeature(category => destination.ConversionResult(category, this), this)
                    : null;

        bool HasForcedConversion(ArrayReferenceType destination)
        {
            if(this == destination)
                return true;

            if(destination.options.IsMutable.Value && !options.IsMutable.Value)
                return false;

            if(ValueType == destination.ValueType)
                return true;

            if(ValueType == destination.ValueType)
                NotImplementedMethod(destination);

            return options.IsEnableReinterpretation.Value;
        }

        Result ConversionResult(Category category, ArrayReferenceType source)
            => Result(category, source.ArgResult);

        Result AccessResult(Category category, TypeBase right)
        {
            var objectResult = ObjectResult(category);
            var dereferencedAlignedResult = objectResult.DereferencedAlignedResult();
            var result = AccessType.Result(category, dereferencedAlignedResult, right);
            if(false && category.HasCode)
                NotImplementedMethod
                    (
                        category,
                        right,
                        nameof(objectResult),
                        objectResult,
                        nameof(dereferencedAlignedResult),
                        dereferencedAlignedResult,
                        nameof(result),
                        result)
                    ;

            return result;
        }

        Result PlusResult(Category category, TypeBase right)
        {
            var conversion = AccessType.Conversion(category, ValueType.Pointer);
            var result = Result(category, conversion);
            var accessResult = AccessType.Result(category, ObjectResult(category).DereferencedAlignedResult(), right);
            NotImplementedMethod
                (
                    category,
                    right,
                    nameof(conversion),
                    conversion,
                    nameof(result),
                    result,
                    nameof(accessResult),
                    accessResult);
            return result.ReplaceArg(accessResult);
        }
        Result MinusResult(Category category, TypeBase right)
        {
            NotImplementedMethod(category, right);
            return null;
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
    }
}