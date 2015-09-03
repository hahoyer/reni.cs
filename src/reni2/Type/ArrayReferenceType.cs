using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
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
            , ISymbolProvider<Mutable>
            , ISymbolProvider<EnableReinterpretation>
            , ISymbolProvider<Plus>
            , ISymbolProvider<Minus>
            , IForcedConversionProvider<ArrayReferenceType>
            , IRepeaterType
            , IReference
            , IValue
    {
        internal sealed class Options : DumpableObject
        {
            OptionsData OptionsData { get; }

            Options(string optionsId)
            {
                OptionsData = new OptionsData(optionsId);
                IsForceMutable = new OptionsData.Option(OptionsData, "force_mutable");
                IsMutable = new OptionsData.Option(OptionsData, "mutable");
                IsEnableReinterpretation = new OptionsData.Option
                    (OptionsData, "enable_reinterpretation");
                OptionsData.Align();
                Tracer.Assert(OptionsData.IsValid);
            }

            internal OptionsData.Option IsMutable { get; }
            internal OptionsData.Option IsForceMutable { get; }
            internal OptionsData.Option IsEnableReinterpretation { get; }

            internal static Options Create(string optionsId) => new Options(optionsId);

            internal static string ForceMutable(bool value)
                => Create(null).IsForceMutable.SetTo(value);

            protected override string GetNodeDump() => DumpPrintText;
            public string DumpPrintText => OptionsData.DumpPrintText;
        }

        readonly int _order;
        readonly ValueCache<RepeaterAccessType> _repeaterAccessTypeCache;

        internal ArrayReferenceType(TypeBase valueType, string optionsId)
        {
            _order = CodeArgs.NextOrder++;
            options = Options.Create(optionsId);
            _repeaterAccessTypeCache = new ValueCache<RepeaterAccessType>
                (() => new RepeaterAccessType(this));
            ValueType = valueType;
            Tracer.Assert(!valueType.Hllw, valueType.Dump);
            Tracer.Assert(!(valueType.CoreType is PointerType), valueType.Dump);

            StopByObjectId(-10);
        }

        [DisableDump]
        internal TypeBase ValueType { get; }
        Options options { get; }

        [DisableDump]
        internal override Root RootContext => ValueType.RootContext;
        internal override string DumpPrintText
            => "(" + ValueType.DumpPrintText + ")reference" + options.DumpPrintText;
        [DisableDump]
        RepeaterAccessType AccessType => _repeaterAccessTypeCache.Value;
        [DisableDump]
        internal override bool Hllw => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;
        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> Genericize
            => this.GenericListFromType(base.Genericize);
        [DisableDump]
        protected override IEnumerable<IValue> RawSymmetricConversions
            => base.RawSymmetricConversions;

        protected override string GetNodeDump()
            => ValueType.NodeDump + "[array_reference]" + options.NodeDump;

        protected override Size GetSize() => ValueType.Pointer.Size;

        [DisableDump]
        internal bool IsMutable => options.IsMutable.Value;
        [DisableDump]
        internal ArrayReferenceType Mutable
            => ValueType.ArrayReference(options.IsMutable.SetTo(true));
        [DisableDump]
        internal ArrayReferenceType EnableReinterpretation
            => ValueType.ArrayReference(options.IsEnableReinterpretation.SetTo(true));

        TypeBase IRepeaterType.ElementType => ValueType;
        TypeBase IRepeaterType.IndexType => RootContext.BitType.Number(Size.ToInt());
        bool IRepeaterType.IsMutable => options.IsForceMutable.Value;

        IValue IReference.Converter => this;
        bool IReference.IsWeak => false;
        Size IContextReference.Size => Size;
        int IContextReference.Order => _order;

        TypeBase IValue.Source => ValueType;
        Result IValue.Result(Category category) => DereferenceResult(category);

        IEnumerable<IValue> IForcedConversionProvider<ArrayReferenceType>.Result
            (ArrayReferenceType destination)
            => ForcedConversion(destination).NullableToArray();

        IImplementation ISymbolProvider<Mutable>.Feature
            (Mutable tokenClass)
            => Feature.Extension.Value(MutableResult);

        IImplementation ISymbolProvider<EnableReinterpretation>.
            Feature
            (EnableReinterpretation tokenClass)
            => Feature.Extension.Value(EnableReinterpretationResult);

        [DisableDump]
        internal override IImplementation FuncionDeclarationForType
            => Feature.Extension.FunctionFeature(AccessResult);

        IImplementation ISymbolProvider<Minus>.Feature
            (Minus tokenClass)
            => Feature.Extension.FunctionFeature(MinusResult);

        IImplementation ISymbolProvider<Plus>.Feature
            (Plus tokenClass)
            => Feature.Extension.FunctionFeature(PlusResult);


        Result MutableResult(Category category)
        {
            Tracer.Assert(options.IsForceMutable.Value);
            return ResultFromPointer(category, Mutable);
        }

        Result EnableReinterpretationResult(Category category)
            => ResultFromPointer(category, EnableReinterpretation);

        IValue ForcedConversion(ArrayReferenceType destination)
            =>
                HasForcedConversion(destination)
                    ? Feature.Extension.Value
                        (category => destination.ConversionResult(category, this), this)
                    : null;

        bool HasForcedConversion(ArrayReferenceType destination)
        {
            if(this == destination)
                return true;

            if(destination.IsMutable && !IsMutable)
                return false;

            if(ValueType == destination.ValueType)
                return true;

            if(ValueType == destination.ValueType)
                NotImplementedMethod(destination);

            return options.IsEnableReinterpretation.Value;
        }

        Result ConversionResult(Category category, ArrayReferenceType source)
            => Result(category, source.ArgResult);

        internal Result ConversionResult(Category category, ArrayType source)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, source);
            try
            {
                return ReturnMethodDump(Result(category, source.Pointer.ArgResult));
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result AccessResult(Category category, TypeBase right)
            => AccessType
                .Result(category, ObjectResult(category).DereferencedAlignedResult(), right);

        Result PlusResult(Category category, TypeBase right)
        {
            var codeAndExts = AccessResult(category, right).DereferenceResult;
            return Result(category, codeAndExts);
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