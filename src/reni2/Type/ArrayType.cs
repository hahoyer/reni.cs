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
using Reni.Parser;
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
    {
        [Node]
        [SmartNode]
        readonly ValueCache<RepeaterAccessType> _repeaterAccessTypeCache;
        [Node]
        [SmartNode]
        readonly ValueCache<NumberType> _numberCache;

        internal sealed class Options : DumpableObject
        {
            OptionsData OptionsData { get; }

            Options(string optionsId)
            {
                OptionsData = new OptionsData(optionsId);
                IsMutable = new OptionsData.Option(OptionsData, "mutable");
                IsTextItem = new OptionsData.Option(OptionsData, "text_item");
                OptionsData.Align();
                Tracer.Assert(OptionsData.IsValid);
            }

            public OptionsData.Option IsMutable { get; }
            public OptionsData.Option IsTextItem { get; }

            public static Options Create(string optionsId = null) => new Options(optionsId);
            internal static readonly string DefaultOptionsId = Create().OptionsData.Id;

            protected override string GetNodeDump() => DumpPrintText;
            public string DumpPrintText => OptionsData.DumpPrintText;
        }

        public ArrayType(TypeBase elementType, int count, string optionsId)
        {
            ElementType = elementType;
            Count = count;
            OptionsValue = Options.Create(optionsId);
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.CheckedReference == null);
            Tracer.Assert(!elementType.Hllw);
            _repeaterAccessTypeCache = new ValueCache<RepeaterAccessType>
                (() => new RepeaterAccessType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
        }

        [DisableDump]
        internal TypeBase ElementType { get; }
        int Count { get; }
        Options OptionsValue { get; }

        TypeBase IRepeaterType.ElementType => ElementType;
        TypeBase IRepeaterType.IndexType => RootContext.BitType.Number(IndexSize.ToInt());
        bool IRepeaterType.IsMutable => IsMutable;
        [DisableDump]
        RepeaterAccessType AccessType => _repeaterAccessTypeCache.Value;
        [DisableDump]
        bool IsMutable => OptionsValue.IsMutable.Value;
        [DisableDump]
        internal NumberType Number => _numberCache.Value;
        [DisableDump]
        ArrayType NoTextItem => ElementType.Array(Count, OptionsValue.IsTextItem.SetTo(false));
        [DisableDump]
        internal ArrayType TextItem => ElementType.Array(Count, OptionsValue.IsTextItem.SetTo(true));
        [DisableDump]
        internal ArrayType Mutable => ElementType.Array(Count, OptionsValue.IsMutable.SetTo(true));

        internal ArrayReferenceType Reference(bool isForceMutable)
            => ElementType.ArrayReference(ArrayReferenceType.Options.ForceMutable(isForceMutable));

        [DisableDump]
        internal override bool Hllw => Count == 0 || ElementType.Hllw;

        internal override string DumpPrintText
            => "(" + ElementType.DumpPrintText + ")*" + Count + OptionsValue.DumpPrintText;

        [DisableDump]
        internal override Size SimpleItemSize
            => OptionsValue.IsTextItem.Value ? (ElementType.SimpleItemSize ?? Size) : base.SimpleItemSize
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

        IImplementation ISymbolProviderForPointer<ToNumberOfBase>.
            Feature
            (ToNumberOfBase tokenClass)
            => OptionsValue.IsTextItem.Value ? Feature.Extension.MetaFeature(ToNumberOfBaseResult) : null;

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
        internal override Result Destructor(Category category)
            => ElementType.ArrayDestructor(category);
        internal override Result Copier(Category category)
            => ElementType.ArrayCopier(category);

        [DisableDump]
        internal override IEnumerable<IConversion> StripConversions
        {
            get { yield return Feature.Extension.Conversion(NoTextItemResult); }
        }

        Result NoTextItemResult(Category category) => ResultFromPointer(category, NoTextItem);
        Result TextItemResult(Category category) => ResultFromPointer(category, TextItem);
        Result MutableResult(Category category) => ResultFromPointer(category, Mutable);
        Result ReferenceResult(Category category)
            => ResultFromPointer(category, Reference(IsMutable));

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
        internal override Root RootContext => ElementType.RootContext;

        [DisableDump]
        TypeBase IndexType => RootContext.BitType.Number(IndexSize.ToInt());

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

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .Array(Count + newCount, argsOptions)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);

        Result DumpPrintTokenArrayResult(Category category)
        {
            var result = RootContext.ConcatPrintResult(category, Count, DumpPrintResult);
            if(category.HasCode)
                result.Code = CodeBase.DumpPrintText("<<" + (OptionsValue.IsMutable.Value ? ":=" : ""))
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
            (Category category, ResultCache left, ContextBase context, CompileSyntax right)
        {
            var target = (left & Category.All)
                .DereferencedAlignedResult()
                .Evaluate(context.RootContext.ExecutionContext)
                .ToString(ElementType.Size);
            var conversionBase = right.Evaluate(context).ToInt32();
            Tracer.Assert(conversionBase >= 2, conversionBase.ToString);
            var result = BitsConst.Convert(target, conversionBase);
            return RootContext.BitType.Result(category, result).Align;
        }

        Result CountResult
            (Category category, ResultCache left, ContextBase context, CompileSyntax right)
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
    }
}