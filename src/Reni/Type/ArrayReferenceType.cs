using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
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
            , ISymbolProviderForPointer<DumpPrintToken>
            , ISymbolProviderForPointer<Mutable>
            , ISymbolProviderForPointer<EnableReinterpretation>
            , ISymbolProviderForPointer<Plus>
            , ISymbolProviderForPointer<Minus>
            , IForcedConversionProvider<ArrayReferenceType>
            , IRepeaterType
            , IChild<TypeBase>
    {
        internal sealed class Options : DumpableObject
        {
            Flags Data { get; }

            Options(string optionsId)
            {
                Data = new Flags(optionsId);
                IsForceMutable = Data.Register("force_mutable");
                IsMutable = Data.Register("mutable");
                IsEnableReinterpretation = Data.Register("enable_reinterpretation");
                Data.Align();
                Data.IsValid.Assert();
            }

            internal Flag IsMutable { get; }
            internal Flag IsForceMutable { get; }
            internal Flag IsEnableReinterpretation { get; }

            internal static Options Create(string optionsId) => new Options(optionsId);

            internal static string ForceMutable(bool value)
                => Create(null).IsForceMutable.SetTo(value);

            protected override string GetNodeDump() => DumpPrintText;
            public string DumpPrintText => Data.DumpPrintText;
        }

        [UsedImplicitly]
        readonly int Order;
        readonly ValueCache<RepeaterAccessType> RepeaterAccessTypeCache;

        internal ArrayReferenceType(TypeBase valueType, string optionsId)
        {
            Order = Closures.NextOrder++;
            OptionsValue = Options.Create(optionsId);
            RepeaterAccessTypeCache = new ValueCache<RepeaterAccessType>
                (() => new RepeaterAccessType(this));
            ValueType = valueType;
            (!valueType.IsHollow).Assert(valueType.Dump);
            (!(valueType.CoreType is PointerType)).Assert(valueType.Dump);

            StopByObjectIds(-10);
        }

        [DisableDump]
        internal TypeBase ValueType { get; }
        Options OptionsValue { get; }

        [DisableDump]
        internal override Root Root => ValueType.Root;
        internal override string DumpPrintText
            => "(" + ValueType.DumpPrintText + ")reference" + OptionsValue.DumpPrintText;

        internal string DumpOptions => OptionsValue.DumpPrintText;

        [DisableDump]
        RepeaterAccessType AccessType => RepeaterAccessTypeCache.Value;
        [DisableDump]
        internal override bool IsHollow => false;
        [DisableDump]
        internal override bool IsAligningPossible => false;

        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        static IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                yield return DumpPrintToken.TokenId;
                yield return TokenClasses.Mutable.TokenId;
                yield return TokenClasses.EnableReinterpretation.TokenId;
                yield return Plus.TokenId;
                yield return Minus.TokenId;
            }
        }

        [DisableDump]
        protected override IEnumerable<IGenericProviderForType> GenericList
            => this.GenericListFromType(base.GenericList);
        [DisableDump]
        protected override IEnumerable<IConversion> RawSymmetricConversions
            => base.RawSymmetricConversions;

        internal override Size SimpleItemSize => ValueType.Size;

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);

        protected override string GetNodeDump()
            => ValueType.NodeDump + "[array_reference]" + OptionsValue.NodeDump;

        protected override Size GetSize() => ValueType.Pointer.Size;

        [DisableDump]
        internal bool IsMutable => OptionsValue.IsMutable.Value;
        [DisableDump]
        internal ArrayReferenceType Mutable
            => ValueType.ArrayReference(OptionsValue.IsMutable.SetTo(true));
        [DisableDump]
        internal ArrayReferenceType EnableReinterpretation
            => ValueType.ArrayReference(OptionsValue.IsEnableReinterpretation.SetTo(true));

        TypeBase IRepeaterType.ElementType => ValueType;
        TypeBase IRepeaterType.IndexType => Root.BitType.Number(Size.ToInt());
        bool IRepeaterType.IsMutable => OptionsValue.IsForceMutable.Value;

        IImplementation ISymbolProviderForPointer<DumpPrintToken>.Feature(DumpPrintToken tokenClass)
            => Feature.Extension.Value(DumpPrintTokenResult);

        IEnumerable<IConversion> IForcedConversionProvider<ArrayReferenceType>.Result
            (ArrayReferenceType destination)
            => ForcedConversion(destination).NullableToArray();

        IImplementation ISymbolProviderForPointer<Mutable>.Feature
            (Mutable tokenClass)
            => Feature.Extension.Value(MutableResult);

        IImplementation ISymbolProviderForPointer<EnableReinterpretation>.
            Feature
            (EnableReinterpretation tokenClass)
            => Feature.Extension.Value(EnableReinterpretationResult);

        [DisableDump]
        internal override IImplementation FunctionDeclarationForPointerType
            => Feature.Extension.FunctionFeature(AccessResult);

        IImplementation ISymbolProviderForPointer<Minus>.Feature
            (Minus tokenClass)
            => Feature.Extension.FunctionFeature(MinusResult);

        IImplementation ISymbolProviderForPointer<Plus>.Feature
            (Plus tokenClass)
            => Feature.Extension.FunctionFeature(PlusResult);


        Result MutableResult(Category category)
        {
            OptionsValue.IsForceMutable.Value.Assert();
            return ResultFromPointer(category, Mutable);
        }

        Result EnableReinterpretationResult(Category category)
            => ResultFromPointer(category, EnableReinterpretation);

        IConversion ForcedConversion(ArrayReferenceType destination)
            =>
                HasForcedConversion(destination)
                    ? Feature.Extension.Conversion
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

            return OptionsValue.IsEnableReinterpretation.Value;
        }

        Result ConversionResult(Category category, ArrayReferenceType source)
            => source.Mutation(this) & category;

        internal Result ConversionResult(Category category, ArrayType source)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, source);
            try
            {
                return ReturnMethodDump(source.Pointer.Mutation(this) & category);
            }
            finally
            {
                EndMethodDump();
            }
        }

        Result AccessResult(Category category, TypeBase right)
        {
            var leftResult = ObjectResult(category).DereferenceResult;
            return AccessType
                .Result(category, leftResult, right);
        }

        Result PlusResult(Category category, TypeBase right)
        {
            var codeAndClosures = AccessResult(category, right).DereferenceResult;
            return Result(category, codeAndClosures);
        }

        Result MinusResult(Category category, TypeBase right)
        {
            NotImplementedMethod(category, right);
            return null;
        }

        TypeBase IChild<TypeBase>.Parent => ValueType;

    }
}