using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;

using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ArrayType
        : TypeBase
            , ISymbolProviderForPointer<DumpPrintToken>
            , ISymbolProviderForPointer<ConcatArrays>
            , ISymbolProviderForPointer<TextItem>
            , ISymbolProviderForPointer<ToNumberOfBase>
            , ISymbolProviderForPointer<Mutable>
            , ISymbolProviderForPointer<ArrayReference>
            , ISymbolProviderForPointer<Count>
            , IForcedConversionProviderForPointer<ArrayReferenceType>
            , IRepeaterType
            , IChild<TypeBase>
    {
        [Node]
        [SmartNode]
        readonly ValueCache<RepeaterAccessType> _repeaterAccessTypeCache;
        [Node]
        [SmartNode]
        readonly ValueCache<NumberType> _numberCache;

        internal sealed class Options : DumpableObject
        {
            Flags Data { get; }

            Options(string optionsId)
            {
                Data = new Flags(optionsId);
                IsMutable = Data.Register("mutable");
                IsTextItem = Data.Register("text_item");
                Data.Align();
                Tracer.Assert(Data.IsValid);
            }

            public Flag IsMutable { get; }
            public Flag IsTextItem { get; }

            public static Options Create(string optionsId = null) => new Options(optionsId);
            internal static readonly string DefaultOptionsId = Create().Data.Id;

            protected override string GetNodeDump() => DumpPrintText;
            public string DumpPrintText => Data.DumpPrintText;
        }

        public ArrayType(TypeBase elementType, int count, string optionsId)
        {
            ElementType = elementType;
            Count = count;
            OptionsValue = Options.Create(optionsId);
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.CheckedReference == null);
            Tracer.Assert(!elementType.IsHollow);
            _repeaterAccessTypeCache = new ValueCache<RepeaterAccessType>
                (() => new RepeaterAccessType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
        }

        [DisableDump]
        internal TypeBase ElementType { get; }
        internal int Count { get; }
        Options OptionsValue { get; }

        TypeBase IRepeaterType.ElementType => ElementType;
        TypeBase IRepeaterType.IndexType => Root.BitType.Number(IndexSize.ToInt());
        bool IRepeaterType.IsMutable => IsMutable;
        [DisableDump]
        RepeaterAccessType AccessType => _repeaterAccessTypeCache.Value;
        [DisableDump]
        internal bool IsMutable => OptionsValue.IsMutable.Value;
        [DisableDump]
        internal bool IsTextItem => OptionsValue.IsTextItem.Value;
        [DisableDump]
        internal NumberType Number => _numberCache.Value;
        [DisableDump]
        ArrayType NoTextItem => ElementType.Array(Count, OptionsValue.IsTextItem.SetTo(false));
        [DisableDump]
        internal ArrayType TextItem => ElementType.Array(Count, OptionsValue.IsTextItem.SetTo(true))
            ;
        [DisableDump]
        internal ArrayType Mutable => ElementType.Array(Count, OptionsValue.IsMutable.SetTo(true));

        internal ArrayReferenceType Reference(bool isForceMutable)
            => ElementType.ArrayReference(ArrayReferenceType.Options.ForceMutable(isForceMutable));

        [DisableDump]
        internal override bool IsHollow => Count == 0 || ElementType.IsHollow;

        internal override string DumpPrintText
            => "(" + ElementType.DumpPrintText + ")*" + Count + OptionsValue.DumpPrintText;

        [DisableDump]
        internal override Size SimpleItemSize
            =>
                OptionsValue.IsTextItem.Value
                    ? (ElementType.SimpleItemSize ?? Size)
                    : base.SimpleItemSize
            ;

        IImplementation ISymbolProviderForPointer<DumpPrintToken>.
            Feature
            (DumpPrintToken tokenClass)
            =>
                OptionsValue.IsTextItem.Value
                    ? Feature.Extension.Value(DumpPrintTokenResult)
                    : Feature.Extension.Value(DumpPrintTokenArrayResult);

        IImplementation ISymbolProviderForPointer<ConcatArrays>.
            Feature(ConcatArrays tokenClass)
            =>
                Feature.Extension.FunctionFeature
                    (
                        (category, objectReference, argsType) =>
                            ConcatArraysResult
                                (
                                    category,
                                    objectReference,
                                    argsType,
                                    OptionsValue.IsMutable.SetTo(tokenClass.IsMutable)),
                        this);

        IImplementation ISymbolProviderForPointer<TextItem>.Feature
            (TextItem tokenClass)
            => Feature.Extension.Value(TextItemResult);

        IImplementation ISymbolProviderForPointer<Mutable>.Feature
            (Mutable tokenClass)
            => Feature.Extension.Value(MutableResult);

        [DisableDump]
        internal override IImplementation FunctionDeclarationForPointerType
            => Feature.Extension.FunctionFeature(ElementAccessResult);

        internal override IEnumerable<string> DeclarationOptions
            => base.DeclarationOptions.Concat(InternalDeclarationOptions);

        IEnumerable<string> InternalDeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }


        IImplementation ISymbolProviderForPointer<ToNumberOfBase>.
            Feature
            (ToNumberOfBase tokenClass)
            =>
                OptionsValue.IsTextItem.Value
                    ? Feature.Extension.MetaFeature(ToNumberOfBaseResult)
                    : null;

        IImplementation ISymbolProviderForPointer<ArrayReference>.
            Feature
            (ArrayReference tokenClass)
            => Feature.Extension.Value(ReferenceResult);

        internal override int? SmartArrayLength(TypeBase elementType)
            => ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType);

        IImplementation ISymbolProviderForPointer<Count>.Feature
            (Count tokenClass)
            => Feature.Extension.MetaFeature(CountResult);

        IEnumerable<IConversion> IForcedConversionProviderForPointer<ArrayReferenceType>.Result
            (ArrayReferenceType destination)
            => ForcedConversion(destination).NullableToArray();

        protected override Size GetSize() => ElementType.Size * Count;

        internal override Result Copier(Category category)
            => ElementType.ArrayCopier(category);

        internal override Result Cleanup(Category category)
            => ElementType.ArrayCleanup(category);

        [DisableDump]
        protected override IEnumerable<IConversion> StripConversions
        {
            get { yield return Feature.Extension.Conversion(NoTextItemResult); }
        }

        Result NoTextItemResult(Category category) => ResultFromPointer(category, NoTextItem);
        Result TextItemResult(Category category) => ResultFromPointer(category, TextItem);
        Result MutableResult(Category category) => ResultFromPointer(category, Mutable);

        Result ReferenceResult(Category category)
            => Reference(IsMutable).Result(category, ObjectResult);

        internal override Result ConstructorResult(Category category, TypeBase argsType)
        {
            if(category.IsNone)
                return null;

            if(argsType == VoidType)
                return Result(category, () => CodeBase.BitsConst(Size, BitsConst.Convert(0)));

            var function = argsType as IFunction;
            if(function != null)
                return ConstructorResult(category, function);

            return base.ConstructorResult(category, argsType);
        }

        Result ConstructorResult(Category category, IFunction function)
        {
            var indexType = BitType
                .Number(BitsConst.Convert(Count).Size.ToInt())
                .Align;
            var constructorResult = function.Result(category.Typed, indexType);
            var elements = Count
                .Select(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate((c, n) => n + c)
                ?? VoidType.Result(category);
            return Result(category, elements);
        }

        Result ElementConstructorResult
            (Category category, Result elementConstructorResult, int i, TypeBase indexType)
        {
            var resultForArg = indexType
                .Result
                (category.Typed, () => CodeBase.BitsConst(indexType.Size, BitsConst.Convert(i)));
            return elementConstructorResult
                .ReplaceArg(resultForArg)
                .Conversion(ElementAccessType)
                .Conversion(ElementType)
                & category;
        }

        TypeBase ElementAccessType => ElementType.TypeForArrayElement;

        [DisableDump]
        internal override Root Root => ElementType.Root;

        [DisableDump]
        TypeBase IndexType => Root.BitType.Number(IndexSize.ToInt());

        Size IndexSize => Size.AutoSize(Count).Align(Root.DefaultRefAlignParam.AlignBits);

        protected override string GetNodeDump()
            => ElementType.NodeDump + "*" + Count + OptionsValue.NodeDump;

        Result ConcatArraysResult
            (
            Category category,
            IContextReference objectReference,
            TypeBase argsType,
            string argsOptions)
        {
            var oldElementsResult = Pointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.Array(newCount, argsOptions));

            var newElementsResult = newElementsResultRaw.AutomaticDereferencedAlignedResult();
            var result = ElementType
                .Array(Count + newCount, argsOptions)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);

        Result DumpPrintTokenArrayResult(Category category)
        {
            var result = Root.ConcatPrintResult(category, Count, DumpPrintResult);
            if(category.HasCode)
                result.Code = CodeBase.DumpPrintText
                    ("<<" + (OptionsValue.IsMutable.Value ? ":=" : ""))
                    + result.Code;
            return result;
        }

        Result DumpPrintResult(Category category, int position)
        {
            return ElementType
                .SmartPointer
                .GenericDumpPrintResult(category)
                .ReplaceAbsolute
                (
                    ElementType.Pointer.CheckedReference,
                    c => ReferenceResult(c).AddToReference(() => ElementType.Size * position)
                );
        }

        Result ElementAccessResult(Category category, TypeBase right)
            => AccessType.Result(category, ObjectResult(category), right);

        Result ToNumberOfBaseResult
            (Category category, ResultCache left, ContextBase context, Parser.Value right)
        {
            var target = (left & Category.All)
                .AutomaticDereferencedAlignedResult()
                .Evaluate(context.RootContext.ExecutionContext)
                .ToString(ElementType.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return Root.BitType.Result(category, result).Align;
        }

        Result CountResult
            (Category category, ResultCache left, ContextBase context, Parser.Value right)
        {
            Tracer.Assert(right == null);
            return IndexType.Result
                (category, () => CodeBase.BitsConst(IndexSize, BitsConst.Convert(Count)));
        }

        IConversion ForcedConversion(ArrayReferenceType destination)
        {
            if(!HasForcedConversion(destination))
                return null;

            return Feature.Extension.Conversion
                (category => destination.ConversionResult(category, this), SmartPointer);
        }

        bool HasForcedConversion(ArrayReferenceType destination)
        {
            if(destination.IsMutable && !OptionsValue.IsMutable.Value)
                return false;

            if(ElementType == destination.ValueType)
                return true;

            NotImplementedMethod(destination);
            return false;
        }

        TypeBase IChild<TypeBase>.Parent => ElementType;
    }
}