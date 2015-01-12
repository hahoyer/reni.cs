using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class ArrayType
        : TypeBase
            , ISymbolProvider<DumpPrintToken, IFeatureImplementation>
            , ISymbolProvider<ConcatArrays, IFeatureImplementation>
            , ISymbolProvider<TextItem, IFeatureImplementation>
            , IRepeaterType
            , IFunctionFeature
            , IFeatureImplementation
    {
        internal readonly TypeBase ElementType;
        internal readonly int Count;
        readonly bool _isMutable;

        readonly ValueCache<RepeaterAccessType> _arrayAccessTypeCache;
        readonly ValueCache<EnableArrayOverSizeType> _enableArrayOverSizeTypeCache;
        readonly ValueCache<NumberType> _numberCache;
        readonly ValueCache<TextItemType> _textItemCache;

        public ArrayType(TypeBase elementType, int count, bool isMutable)
        {
            ElementType = elementType;
            Count = count;
            _isMutable = isMutable;
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.ReferenceType == null);
            Tracer.Assert(!elementType.Hllw);
            _arrayAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            _enableArrayOverSizeTypeCache = new ValueCache<EnableArrayOverSizeType>(() => new EnableArrayOverSizeType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
            _textItemCache = new ValueCache<TextItemType>(() => new TextItemType(this));
        }

        TypeBase IRepeaterType.ElementType => ElementType;
        Size IRepeaterType.IndexSize => IndexSize;
        bool IRepeaterType.IsMutable => _isMutable;

        [DisableDump]
        public NumberType Number => _numberCache.Value;

        [DisableDump]
        internal TextItemType TextItemType => _textItemCache.Value;

        [DisableDump]
        internal EnableArrayOverSizeType EnableArrayOverSizeType => _enableArrayOverSizeTypeCache.Value;

        [DisableDump]
        internal override bool Hllw => Count == 0 || ElementType.Hllw;

        [DisableDump]
        public override TypeBase ArrayElementType => ElementType;

        internal override string DumpPrintText => "(" + ElementType.DumpPrintText + ")*" + Count;

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
            => Extension.SimpleFeature(DumpPrintTokenResult);

        IFeatureImplementation ISymbolProvider<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
            =>
                Extension.FunctionFeature
                    (
                        (category, objectReference, argsType) =>
                            ConcatArraysResult(category, objectReference, argsType, tokenClass.IsMutable), this);

        IFeatureImplementation ISymbolProvider<TextItem, IFeatureImplementation>.Feature(TextItem tokenClass)
            => Extension.SimpleFeature(TextItemResult);

        internal override int? SmartArrayLength(TypeBase elementType)
            => ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType);

        protected override Size GetSize() => ElementType.Size * Count;
        internal override Result Destructor(Category category) => ElementType.ArrayDestructor(category, Count);
        internal override Result Copier(Category category) => ElementType.ArrayCopier(category, Count);

        internal Result TextItemResult(Category category) => PointerConversionResult(category, TextItemType);

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
        IContextReference ObjectReference => Reference;

        [DisableDump]
        internal TypeBase IndexType => RootContext.BitType.Number(IndexSize.ToInt());

        Size IndexSize => Size.AutoSize(Count).Align(Root.DefaultRefAlignParam.AlignBits);

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta => null;
        IMetaFunctionFeature IFeatureImplementation.Meta => null;
        IFunctionFeature IFeatureImplementation.Function => this;
        ISimpleFeature IFeatureImplementation.Simple => null;

        bool IFunctionFeature.IsImplicit => false;
        IContextReference IFunctionFeature.ObjectReference => ObjectReference;

        protected override string GetNodeDump() => ElementType.NodeDump + "*" + Count;

        internal Result ConcatArraysResult
            (Category category, IContextReference objectReference, TypeBase argsType, bool isMutable)
            => InternalConcatArrays(category, objectReference, argsType, isMutable);

        internal Result ConcatArraysFromReference
            (Category category, IContextReference objectReference, TypeBase argsType, bool isMutable)
            => InternalConcatArrays(category, objectReference, argsType, isMutable);

        internal Result InternalConcatArrays
            (Category category, IContextReference objectReference, TypeBase argsType, bool isMutable)
        {
            var oldElementsResult = Pointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.Array(newCount, isMutable));

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .Array(Count + newCount, isMutable)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        new Result DumpPrintTokenResult(Category category) => VoidType
            .Result
            (
                category,
                CreateDumpPrintCode,
                () => ElementType.GenericDumpPrintResult(Category.Exts).Exts
            );

        CodeBase CreateDumpPrintCode()
        {
            var elementReference = ElementType.Pointer;
            var argCode = Pointer.ArgCode;
            var elementDumpPrint = elementReference.GenericDumpPrintResult(Category.Code).Code;
            var code = CodeBase.DumpPrintText("<<" + (_isMutable ? ":=" : "") + "(");
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    code = code + CodeBase.DumpPrintText(", ");
                var elemCode = elementDumpPrint.ReplaceArg(elementReference, argCode.ReferencePlus(ElementType.Size * i));
                code = code + elemCode;
            }
            return code + CodeBase.DumpPrintText(")");
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) => ApplyResult(category, argsType);
        Result ApplyResult(Category category, TypeBase argsType)
        {
            var objectResult = Pointer
                .Result(category, ObjectReference);

            var argsResult = argsType
                .Conversion(category.Typed, IndexType)
                .DereferencedAlignedResult();

            var result = _arrayAccessTypeCache
                .Value
                .Result(category, objectResult + argsResult);

            return result;
        }

        internal Result EnableArrayOverSizeResult(Category category)
        {
            return EnableArrayOverSizeType
                .Result
                (
                    category,
                    () => SmartPointer.ArgCode.DePointer(Size),
                    CodeArgs.Arg
                );
        }
    }
}