using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Numeric;
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

        readonly ValueCache<RepeaterAccessType> _arrayAccessTypeCache;
        readonly ValueCache<EnableArrayOverSizeType> _enableArrayOverSizeTypeCache;
        readonly ValueCache<NumberType> _numberCache;
        readonly ValueCache<TextItemType> _textItemCache;

        public ArrayType(TypeBase elementType, int count)
        {
            ElementType = elementType;
            Count = count;
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.ReferenceType == null);
            Tracer.Assert(!elementType.Hllw);
            _arrayAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            _enableArrayOverSizeTypeCache = new ValueCache<EnableArrayOverSizeType>(() => new EnableArrayOverSizeType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
            _textItemCache = new ValueCache<TextItemType>(() => new TextItemType(this));
        }

        TypeBase IRepeaterType.ElementType { get { return ElementType; } }
        Size IRepeaterType.IndexSize { get { return IndexSize; } }

        [DisableDump]
        public NumberType UniqueNumber { get { return _numberCache.Value; } }

        [DisableDump]
        internal TextItemType UniqueTextItemType { get { return _textItemCache.Value; } }

        [DisableDump]
        internal EnableArrayOverSizeType EnableArrayOverSizeType { get { return _enableArrayOverSizeTypeCache.Value; } }

        [DisableDump]
        internal override bool Hllw { get { return Count == 0 || ElementType.Hllw; } }

        [DisableDump]
        public override TypeBase ArrayElementType { get { return ElementType; } }

        internal override string DumpPrintText { get { return "(" + ElementType.DumpPrintText + ")*" + Count; } }

        IFeatureImplementation ISymbolProvider<DumpPrintToken, IFeatureImplementation>.Feature(DumpPrintToken tokenClass)
        {
            return Extension.SimpleFeature(DumpPrintTokenResult);
        }

        IFeatureImplementation ISymbolProvider<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
        {
            return Extension.FunctionFeature(ConcatArraysResult);
        }

        IFeatureImplementation ISymbolProvider<TextItem, IFeatureImplementation>.Feature(TextItem tokenClass)
        {
            return Extension.SimpleFeature(TextItemResult);
        }

        internal override int? SmartArrayLength(TypeBase elementType)
        {
            return ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType);
        }
        protected override Size GetSize() { return ElementType.Size * Count; }
        internal override Result Destructor(Category category) { return ElementType.ArrayDestructor(category, Count); }
        internal override Result Copier(Category category) { return ElementType.ArrayCopier(category, Count); }

        internal Result TextItemResult(Category category) 
            => UniqueTextItemType.Result(category, ArgResult(category));

        internal override Result ConstructorResult(Category category, TypeBase argsType)
        {
            return Result
                (
                    category,
                    c => InternalConstructorResult(c, argsType)
                );
        }

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
                .UniqueNumber(BitsConst.Convert(Count).Size.ToInt())
                .UniqueAlign;
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

        TypeBase ElementAccessType { get { return ElementType.TypeForArrayElement; } }

        [DisableDump]
        internal override Root RootContext { get { return ElementType.RootContext; } }

        [DisableDump]
        IContextReference ObjectReference { get { return UniquePointerType; } }

        [DisableDump]
        internal TypeBase IndexType { get { return RootContext.BitType.UniqueNumber(IndexSize.ToInt()); } }

        Size IndexSize { get { return Size.AutoSize(Count).Align(Root.DefaultRefAlignParam.AlignBits); } }

        IContextMetaFunctionFeature IFeatureImplementation.ContextMeta { get { return null; } }
        IMetaFunctionFeature IFeatureImplementation.Meta { get { return null; } }
        IFunctionFeature IFeatureImplementation.Function { get { return this; } }
        ISimpleFeature IFeatureImplementation.Simple { get { return null; } }

        bool IFunctionFeature.IsImplicit { get { return false; } }
        IContextReference IFunctionFeature.ObjectReference { get { return ObjectReference; } }

        protected override string GetNodeDump() { return ElementType.NodeDump + "*" + Count; }

        internal Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType)
        {
            return InternalConcatArrays(category, objectReference, argsType);
        }

        internal Result ConcatArraysFromReference(Category category, IContextReference objectReference, TypeBase argsType)
        {
            return InternalConcatArrays(category, objectReference, argsType);
        }

        internal Result InternalConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var oldElementsResult = UniquePointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.UniqueArray(newCount));

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .UniqueArray(Count + newCount)
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
            var elementReference = ElementType.UniquePointer;
            var argCode = UniquePointer.ArgCode;
            var elementDumpPrint = elementReference.GenericDumpPrintResult(Category.Code).Code;
            var code = CodeBase.DumpPrintText("<<(");
            for(var i = 0; i < Count; i++)
            {
                if(i > 0)
                    code = code + CodeBase.DumpPrintText(", ");
                var elemCode = elementDumpPrint.ReplaceArg(elementReference, argCode.ReferencePlus(ElementType.Size * i));
                code = code + elemCode;
            }
            return code + CodeBase.DumpPrintText(")");
        }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return ApplyResult(category, argsType); }
        Result ApplyResult(Category category, TypeBase argsType)
        {
            var objectResult = UniquePointer
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
                    () => PointerKind.ArgCode.DePointer(Size),
                    CodeArgs.Arg
                );
        }
    }
}