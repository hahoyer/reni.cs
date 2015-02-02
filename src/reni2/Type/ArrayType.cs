using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.ReniSyntax;
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
            , ISymbolProviderForPointer<ToNumberOfBase, IFeatureImplementation>
            , IRepeaterType
    {
        internal sealed class Options : DumpableObject
        {
            static int Index(bool isMutable, bool isTextItem, bool isOversizeable)
            {
                var result = 0;
                if(isMutable)
                    result++;
                result *= 2;
                if(isTextItem)
                    result++;
                result *= 2;
                if(isOversizeable)
                    result++;
                return result;
            }

            static IEnumerable<Options> CreateInstances()
            {
                yield return new Options(false, false, false);
                yield return new Options(false, false, true);
                yield return new Options(false, true, false);
                yield return new Options(false, true, true);
                yield return new Options(true, false, false);
                yield return new Options(true, false, true);
                yield return new Options(true, true, false);
                yield return new Options(true, true, true);
            }

            internal static readonly Options[] Instances = CreateInstances().ToArray();

            Options(bool isMutable, bool isTextItem, bool isOversizeable)
            {
                IsMutable = isMutable;
                IsTextItem = isTextItem;
                IsOversizeable = isOversizeable;
            }

            public bool IsMutable { get; }
            public bool IsTextItem { get; }
            public bool IsOversizeable { get; }

            IEnumerable<string> Tags
            {
                get
                {
                    if(IsMutable)
                        yield return EnableReassignToken.Id;
                    if(IsTextItem)
                        yield return TokenClasses.TextItem.Id;
                    if(IsOversizeable)
                        yield return EnableArrayOverSize.Id;
                }
            }

            protected override string GetNodeDump() => Tags.Select(t => "[" + t + "]").Stringify("");
            public Options ToTextItem(bool value = true) => Instance(IsMutable, value, IsOversizeable);
            public Options ToMutable(bool value = true) => Instance(value, IsTextItem, IsOversizeable);
            public Options ToEnableArrayOverSize(bool value = true) => Instance(IsMutable, IsTextItem, value);

            public static Options Instance(bool isMutable = false, bool isTextItem = false, bool isOversizeable = false)
                => Instances[Index(isMutable, isTextItem, isOversizeable)];
        }


        readonly ValueCache<RepeaterAccessType> _arrayAccessTypeCache;
        readonly ValueCache<NumberType> _numberCache;

        public ArrayType(TypeBase elementType, int count, Options option)
        {
            ElementType = elementType;
            Count = count;
            Option = option;
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.CheckedReference == null);
            Tracer.Assert(!elementType.Hllw);
            _arrayAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
        }

        internal TypeBase ElementType { get; }
        int Count { get; }
        Options Option { get; }

        TypeBase IRepeaterType.ElementType => ElementType;
        Size IRepeaterType.IndexSize => IndexSize;
        bool IRepeaterType.IsMutable => Option.IsMutable;

        [DisableDump]
        public NumberType Number => _numberCache.Value;

        [DisableDump]
        internal ArrayType TextItem => ElementType.Array(Count, Option.ToTextItem());
        [DisableDump]
        internal ArrayType Mutable => ElementType.Array(Count, Option.ToMutable());

        [DisableDump]
        ArrayType EnableArrayOverSizeType => ElementType.Array(Count, Option.ToEnableArrayOverSize());

        [DisableDump]
        internal override bool Hllw => Count == 0 || ElementType.Hllw;

        internal override string DumpPrintText => "(" + ElementType.DumpPrintText + ")*" + Count;

        [DisableDump]
        internal override Size SimpleItemSize => Option.IsTextItem ? (ElementType.SimpleItemSize ?? Size) : base.SimpleItemSize;

        IFeatureImplementation ISymbolProviderForPointer<EnableArrayOverSize, IFeatureImplementation>.Feature
            (EnableArrayOverSize tokenClass)
            => Extension.SimpleFeature(EnableArrayOverSizeResult);

        IFeatureImplementation ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>.Feature
            (DumpPrintToken tokenClass)
            =>
                Option.IsTextItem
                    ? Extension.SimpleFeature(DumpPrintTokenResult)
                    : Extension.SimpleFeature(DumpPrintTokenArrayResult);

        IFeatureImplementation ISymbolProviderForPointer<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
            =>
                Extension.FunctionFeature
                    (
                        (category, objectReference, argsType) =>
                            ConcatArraysResult
                                (category, objectReference, argsType, Option.ToMutable(tokenClass.IsMutable)),
                        this);

        IFeatureImplementation ISymbolProviderForPointer<TextItem, IFeatureImplementation>.Feature(TextItem tokenClass)
            => Extension.SimpleFeature(TextItemResult);

        IFeatureImplementation ISymbolProviderForPointer<TokenClasses.ArrayAccess, IFeatureImplementation>.Feature
            (TokenClasses.ArrayAccess tokenClass)
            => Extension.FunctionFeature(ElementAccessResult);

        IFeatureImplementation ISymbolProviderForPointer<ToNumberOfBase, IFeatureImplementation>.Feature
            (ToNumberOfBase tokenClass)
            => Option.IsTextItem ? Extension.MetaFeature(ToNumberOfBaseResult) : null;


        internal override int? SmartArrayLength(TypeBase elementType)
            => ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType);

        protected override Size GetSize() => ElementType.Size * Count;
        internal override Result Destructor(Category category) => ElementType.ArrayDestructor(category, Count);
        internal override Result Copier(Category category) => ElementType.ArrayCopier(category, Count);

        Result TextItemResult(Category category) => ResultFromPointer(category, TextItem);
        Result EnableArrayOverSizeResult(Category category) => ResultFromPointer(category, EnableArrayOverSizeType);

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

        protected override string GetNodeDump() => ElementType.NodeDump + "*" + Count + Option.NodeDump;

        Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType, Options options)
        {
            var oldElementsResult = Pointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.Array(newCount, Option));

            var newElementsResult = newElementsResultRaw.DereferencedAlignedResult();
            var result = ElementType
                .Array(Count + newCount, options)
                .Result(category, newElementsResult + oldElementsResult);
            return result;
        }

        protected override CodeBase DumpPrintCode() => ArgCode.DumpPrintText(SimpleItemSize);

        Result DumpPrintTokenArrayResult(Category category)
        {
            var result = RootContext.ConcatPrintResult(category, Count, DumpPrintResult);
            if(!category.HasCode)
                return result;

            result.Code = CodeBase.DumpPrintText("<<" + (Option.IsMutable ? ":=" : "")) + result.Code;
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

        Result ToNumberOfBaseResult(Category category, ResultCache left, ContextBase context, CompileSyntax right)
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
    }
}