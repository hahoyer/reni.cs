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
    sealed class ArrayType
        : TypeBase
            , ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>
            , ISymbolProviderForPointer<ConcatArrays, IFeatureImplementation>
            , ISymbolProviderForPointer<TextItem, IFeatureImplementation>
            , ISymbolProviderForPointer<TokenClasses.ArrayAccess, IFeatureImplementation>
            , ISymbolProviderForPointer<EnableArrayOverSize, IFeatureImplementation>
            , IRepeaterType
    {
        sealed internal class Options : DumpableObject
        {
            public static readonly Options None = new Options(false, false, false);

            Options(bool isMutable, bool isTextItem, bool isOversizeable)
            {
                IsMutable = isMutable;
                IsTextItem = isTextItem;
                IsOversizeable = isOversizeable;
                Mutable = isMutable ? this :  new Options(true, isTextItem, isOversizeable);
            }

            public bool IsMutable { get; }
            public bool IsTextItem { get; }
            public bool IsOversizeable { get; }
            public Options Mutable;

            IEnumerable<string> Tags
            {
                get
                {
                    if (IsMutable)
                        yield return EnableReassignToken.Id;
                    if (IsTextItem)
                        yield return TextItem.Id;
                    if (IsOversizeable)
                        yield return EnableArrayOverSize.Id;
                }
            }

            public string DumpPrintText => Tags.Stringify(" ");
            public static Options Instance(bool isMutable) => isMutable ? None.Mutable : None;

        }


        readonly ValueCache<RepeaterAccessType> _arrayAccessTypeCache;
        readonly ValueCache<EnableArrayOverSizeType> _enableArrayOverSizeTypeCache;
        readonly ValueCache<NumberType> _numberCache;
        readonly ValueCache<TextItemType> _textItemCache;

        public ArrayType(TypeBase elementType, int count, Options options)
        {
            ElementType = elementType;
            Count = count;
            this.options = options;
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.CheckedReference == null);
            Tracer.Assert(!elementType.Hllw);
            _arrayAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            _enableArrayOverSizeTypeCache = new ValueCache<EnableArrayOverSizeType>(() => new EnableArrayOverSizeType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
            _textItemCache = new ValueCache<TextItemType>(() => new TextItemType(this));
        }

        internal TypeBase ElementType { get; }
        int Count { get; }
        Options options { get; }

        TypeBase IRepeaterType.ElementType => ElementType;
        Size IRepeaterType.IndexSize => IndexSize;
        bool IRepeaterType.IsMutable => options.IsMutable;

        [DisableDump]
        public NumberType Number => _numberCache.Value;

        [DisableDump]
        internal TextItemType TextItemType => _textItemCache.Value;

        [DisableDump]
        EnableArrayOverSizeType EnableArrayOverSizeType => _enableArrayOverSizeTypeCache.Value;

        [DisableDump]
        internal override bool Hllw => Count == 0 || ElementType.Hllw;

        internal override string DumpPrintText => "(" + ElementType.DumpPrintText + ")*" + Count;

        IFeatureImplementation ISymbolProviderForPointer<EnableArrayOverSize, IFeatureImplementation>.Feature
            (EnableArrayOverSize tokenClass)
            => Extension.SimpleFeature(EnableArrayOverSizeResult);

        IFeatureImplementation ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>.Feature
            (DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProviderForPointer<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
            =>
                Extension.FunctionFeature
                    (
                        (category, objectReference, argsType) =>
                            ConcatArraysResult(category, objectReference, argsType, Options.Instance(isMutable:tokenClass.IsMutable)),
                        this);

        IFeatureImplementation ISymbolProviderForPointer<TextItem, IFeatureImplementation>.Feature(TextItem tokenClass)
            => Extension.SimpleFeature(TextItemResult);

        IFeatureImplementation ISymbolProviderForPointer<TokenClasses.ArrayAccess, IFeatureImplementation>.Feature
            (TokenClasses.ArrayAccess tokenClass)
            => Extension.FunctionFeature(ElementAccessResult);

        internal override int? SmartArrayLength(TypeBase elementType)
            => ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType);

        protected override Size GetSize() => ElementType.Size * Count;
        internal override Result Destructor(Category category) => ElementType.ArrayDestructor(category, Count);
        internal override Result Copier(Category category) => ElementType.ArrayCopier(category, Count);

        Result TextItemResult(Category category) => ResultFromPointer(category, TextItemType);

        internal override Result ConstructorResult(Category category, TypeBase argsType) => Result
            (
                category,
                c => InternalConstructorResult(c, argsType)
            );

        Result InternalConstructorResult(Category category, TypeBase argsType)
        {
            if(category.IsNone)
                return null;

            if(argsType == VoidType)
                return Result(category, () => CodeBase.BitsConst(Size, BitsConst.Convert(0)));

            var function = argsType as IFunctionFeature;
            if(function != null)
                return InternalConstructorResult(category, function);

            NotImplementedMethod(category, argsType);
            return null;
        }

        Result InternalConstructorResult(Category category, IFunctionFeature function)
        {
            var indexType = BitType
                .Number(BitsConst.Convert(Count).Size.ToInt())
                .Align;
            var constructorResult = function.ApplyResult(category.Typed, indexType);
            var elements = Count
                .Select(i => ElementConstructorResult(category, constructorResult, i, indexType))
                .Aggregate((c, n) => n + c)
                ?? VoidType.Result(category);
            return Result(category, elements);
        }

        Result ElementConstructorResult(Category category, Result elementConstructorResult, int i, TypeBase indexType)
        {
            var resultForArg = indexType
                .Result(category.Typed, () => CodeBase.BitsConst(indexType.Size, BitsConst.Convert(i)));
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

        protected override string GetNodeDump() => ElementType.NodeDump + "*" + Count;

        Result ConcatArraysResult
            (Category category, IContextReference objectReference, TypeBase argsType, Options options)
            => InternalConcatArrays(category, objectReference, argsType, options);

        internal Result InternalConcatArrays
            (Category category, IContextReference objectReference, TypeBase argsType, Options options)
        {
            var oldElementsResult = Pointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.Array(newCount, this.options));

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .Array(Count + newCount, options)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        new Result DumpPrintTokenResult(Category category)
        {
            var result = RootContext.ConcatPrintResult(category, Count, DumpPrintResult);
            if(!category.HasCode)
                return result;

            result.Code = CodeBase.DumpPrintText("<<" + (options.IsMutable? ":=" :"")) + result.Code;
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

        Result ElementAccessResult(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var objectResult = Pointer.Result(category, objectReference);

            var argsResult = argsType
                .Conversion(category.Typed, IndexType)
                .DereferencedAlignedResult();

            var result = _arrayAccessTypeCache
                .Value
                .Result(category, objectResult + argsResult);

            return result;
        }

        Result EnableArrayOverSizeResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}