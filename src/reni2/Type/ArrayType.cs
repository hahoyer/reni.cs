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
            , ISymbolProviderForPointer<ToNumberOfBase, IFeatureImplementation>
            , ISymbolProviderForPointer<Mutable, IFeatureImplementation>
            , ISymbolProviderForPointer<Reference, IFeatureImplementation>
            , IRepeaterType
    {
        [Node]
        [SmartNode]
        readonly ValueCache<RepeaterAccessType> _arrayAccessTypeCache;
        [Node]
        [SmartNode]
        readonly ValueCache<NumberType> _numberCache;
        [Node]
        [SmartNode]
        readonly FunctionCache<string, ReferenceType> _referenceCache;

        internal sealed class Options : DumpableObject
        {
            OptionsData OptionsData { get; }

            Options(string optionsId)
            {
                OptionsData = new OptionsData(optionsId);
                IsMutable = new OptionsData.Option(OptionsData);
                IsTextItem = new OptionsData.Option(OptionsData);
                OptionsData.Align();
                Tracer.Assert(OptionsData.IsValid);
            }

            public OptionsData.Option IsMutable { get; }
            public OptionsData.Option IsTextItem { get; }

            public static Options Create(string optionsId = null) => new Options(optionsId);
            internal static readonly string DefaultOptionsId = Create().OptionsData.Id;
            protected override string GetNodeDump()
                => (IsMutable.Value ? "m" : "")
                    + (IsTextItem.Value ? "t" : "");
        }

        public ArrayType(TypeBase elementType, int count, string optionsId)
        {
            ElementType = elementType;
            Count = count;
            options = Options.Create(optionsId);
            Tracer.Assert(count > 0);
            Tracer.Assert(elementType.CheckedReference == null);
            Tracer.Assert(!elementType.Hllw);
            _arrayAccessTypeCache = new ValueCache<RepeaterAccessType>(() => new RepeaterAccessType(this));
            _numberCache = new ValueCache<NumberType>(() => new NumberType(this));
            _referenceCache = new FunctionCache<string, ReferenceType>(id => new ReferenceType(this, id));
        }

        TypeBase ElementType { get; }
        int Count { get; }
        Options options { get; }

        TypeBase IRepeaterType.ElementType => ElementType;
        Size IRepeaterType.IndexSize => IndexSize;
        bool IRepeaterType.IsMutable => IsMutable;

        [DisableDump]
        internal bool IsMutable => options.IsMutable.Value;

        [DisableDump]
        public NumberType Number => _numberCache.Value;

        [DisableDump]
        internal ArrayType TextItem => ElementType.Array(Count, options.IsTextItem.SetTo(true));
        [DisableDump]
        internal ArrayType Mutable => ElementType.Array(Count, options.IsMutable.SetTo(true));

        internal ReferenceType Reference(string optionsId=null) => _referenceCache[optionsId ?? ReferenceType.Options.DefaultOptionsId];
        [DisableDump]
        internal override bool Hllw => Count == 0 || ElementType.Hllw;

        internal override string DumpPrintText => "(" + ElementType.DumpPrintText + ")*" + Count;

        [DisableDump]
        internal override Size SimpleItemSize
            => options.IsTextItem.Value ? (ElementType.SimpleItemSize ?? Size) : base.SimpleItemSize;

        IFeatureImplementation ISymbolProviderForPointer<DumpPrintToken, IFeatureImplementation>.Feature
            (DumpPrintToken tokenClass)
            =>
                options.IsTextItem.Value
                    ? Feature.Extension.SimpleFeature(DumpPrintTokenResult)
                    : Feature.Extension.SimpleFeature(DumpPrintTokenArrayResult);

        IFeatureImplementation ISymbolProviderForPointer<ConcatArrays, IFeatureImplementation>.Feature(ConcatArrays tokenClass)
            =>
                Feature.Extension.FunctionFeature
                    (
                        (category, objectReference, argsType) =>
                            ConcatArraysResult
                                (category, objectReference, argsType, options.IsMutable.SetTo(tokenClass.IsMutable)),
                        this);

        IFeatureImplementation ISymbolProviderForPointer<TextItem, IFeatureImplementation>.Feature(TextItem tokenClass)
            => Feature.Extension.SimpleFeature(TextItemResult);

        IFeatureImplementation ISymbolProviderForPointer<Mutable, IFeatureImplementation>.Feature(Mutable tokenClass)
            => Feature.Extension.SimpleFeature(MutableResult);

        IFeatureImplementation ISymbolProviderForPointer<TokenClasses.ArrayAccess, IFeatureImplementation>.Feature
            (TokenClasses.ArrayAccess tokenClass)
            => Feature.Extension.FunctionFeature(ElementAccessResult);

        IFeatureImplementation ISymbolProviderForPointer<ToNumberOfBase, IFeatureImplementation>.Feature
            (ToNumberOfBase tokenClass)
            => options.IsTextItem.Value ? Feature.Extension.MetaFeature(ToNumberOfBaseResult) : null;

        IFeatureImplementation ISymbolProviderForPointer<Reference, IFeatureImplementation>.Feature(Reference tokenClass)
            => Feature.Extension.SimpleFeature(ReferenceResult);

        internal override int? SmartArrayLength(TypeBase elementType)
            => ElementType.IsConvertable(elementType) ? Count : base.SmartArrayLength(elementType);

        protected override Size GetSize() => ElementType.Size * Count;
        internal override Result Destructor(Category category) => ElementType.ArrayDestructor(category, Count);
        internal override Result Copier(Category category) => ElementType.ArrayCopier(category, Count);

        Result TextItemResult(Category category) => ResultFromPointer(category, TextItem);
        Result MutableResult(Category category) => ResultFromPointer(category, Mutable);
        Result ReferenceResult(Category category) => ResultFromPointer(category, Reference());

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

        protected override string GetNodeDump() => ElementType.NodeDump + "*" + Count + options.NodeDump;

        Result ConcatArraysResult(Category category, IContextReference objectReference, TypeBase argsType, string options)
        {
            var oldElementsResult = Pointer
                .Result(category.Typed, objectReference).DereferenceResult;

            var isElementArg = argsType.IsConvertable(ElementAccessType);
            var newCount = isElementArg ? 1 : argsType.ArrayLength(ElementAccessType);
            var newElementsResultRaw
                = isElementArg
                    ? argsType.Conversion(category.Typed, ElementAccessType)
                    : argsType.Conversion(category.Typed, ElementType.Array(newCount, options));

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

            result.Code = CodeBase.DumpPrintText("<<" + (options.IsMutable.Value ? ":=" : "")) + result.Code;
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