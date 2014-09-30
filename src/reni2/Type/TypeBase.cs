using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Type
{
    abstract class TypeBase
        : DumpableObject
            , IContextReferenceProvider
            , IIconKeyProvider
            , ISearchTarget
    {
        sealed class Cache
        {
            [Node]
            [SmartNode]
            public readonly FunctionCache<int, Aligner> Aligner;
            [Node]
            [SmartNode]
            public readonly FunctionCache<int, ArrayType> Array;
            [Node]
            [SmartNode]
            public readonly FunctionCache<TypeBase, Pair> Pair;
            [Node]
            [SmartNode]
            public readonly ValueCache<IReferenceType> Pointer;
            [Node]
            [SmartNode]
            public readonly ValueCache<TypeType> TypeType;
            [Node]
            [SmartNode]
            public readonly ValueCache<FunctionInstanceType> FunctionInstanceType;
            [Node]
            [SmartNode]
            public readonly ValueCache<TextItemType> TextItem;
            [Node]
            [SmartNode]
            public readonly ValueCache<EnableCut> EnableCut;
            public readonly ValueCache<Size> Size;

            public Cache(TypeBase parent)
            {
                EnableCut = new ValueCache<EnableCut>(() => new EnableCut(parent));
                Pointer = new ValueCache<IReferenceType>(parent.ObtainPointer);
                Pair = new FunctionCache<TypeBase, Pair>(first => new Pair(first, parent));
                Array = new FunctionCache<int, ArrayType>(parent.ObtainArray);
                Aligner = new FunctionCache<int, Aligner>(alignBits => new Aligner(parent, alignBits));
                FunctionInstanceType = new ValueCache<FunctionInstanceType>(() => new FunctionInstanceType(parent));
                TypeType = new ValueCache<TypeType>(() => new TypeType(parent));
                TextItem = new ValueCache<TextItemType>(() => new TextItemType(parent));
                Size = new ValueCache<Size>(parent.ObtainSize);
            }
        }

        static int _nextObjectId;
        [Node]
        [SmartNode]
        readonly Cache _cache;

        [DisableDump]
        [Node]
        internal abstract Root RootContext { get; }

        protected TypeBase()
            : base(_nextObjectId++)
        {
            _cache = new Cache(this);
        }


        IContextReference IContextReferenceProvider.ContextReference { get { return UniquePointerType; } }

        [Node]
        internal Size Size { get { return _cache.Size.Value; } }

        [NotNull]
        protected virtual Size GetSize()
        {
            NotImplementedMethod();
            return Size.Zero;
        }

        [NotNull]
        Size ObtainSize()
        {
            if(Hllw)
                return Size.Zero;
            return GetSize();
        }

        [DisableDump]
        internal virtual bool Hllw
        {
            get
            {
                NotImplementedMethod();
                return true;
            }
        }


        [DisableDump]
        internal virtual TypeBase[] ToList { get { return new[] {this}; } }


        [DisableDump]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                return "";
            }
        }

        internal virtual int? SmartSequenceLength(TypeBase elementType) { return SmartArrayLength(elementType); }

        internal virtual int? SmartArrayLength(TypeBase elementType)
        {
            if(IsConvertable(elementType))
                return 1;
            NotImplementedMethod(elementType);
            return null;
        }

        [DisableDump]
        internal TextItemType UniqueTextItemType { get { return _cache.TextItem.Value; } }

        [DisableDump]
        internal EnableCut UniqueEnableCutType { get { return _cache.EnableCut.Value; } }

        [DisableDump]
        internal TypeBase UniquePointer { get { return UniquePointerType.Type(); } }

        [DisableDump]
        internal virtual IReferenceType UniquePointerType { get { return _cache.Pointer.Value; } }

        [DisableDump]
        internal CodeBase ArgCode { get { return CodeBase.Arg(this); } }

        [DisableDump]
        internal TypeBase AutomaticDereferenceType
        {
            get
            {
                if(IsWeakReference)
                    return ReferenceType.Converter.TargetType.AutomaticDereferenceType;
                return this;
            }
        }

        [DisableDump]
        internal TypeBase PointerKind
        {
            get
            {
                if(Hllw)
                    return this;
                return UniquePointer;
            }
        }

        [DisableDump]
        internal TypeBase UniqueAlign
        {
            get
            {
                var alignBits = Root.DefaultRefAlignParam.AlignBits;
                if(Size.Align(alignBits) == Size)
                    return this;
                return _cache.Aligner[alignBits];
            }
        }

        Result VoidCodeAndRefs(Category category) { return RootContext.VoidResult(category & (Category.Code | Category.Exts)); }

        internal ArrayType UniqueArray(int count) { return _cache.Array[count]; }
        protected virtual TypeBase ReversePair(TypeBase first) { return first._cache.Pair[this]; }
        internal virtual TypeBase Pair(TypeBase second) { return second.ReversePair(this); }
        internal virtual Result Destructor(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayDestructor(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result Copier(Category category) { return VoidCodeAndRefs(category); }
        internal virtual Result ArrayCopier(Category category, int count) { return VoidCodeAndRefs(category); }
        internal virtual Result ApplyTypeOperator(Result argResult)
        {
            return argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult);
        }
        internal Result ArgResult(Category category) { return Result(category, () => ArgCode, CodeArgs.Arg); }
        internal Result PointerArgResult(Category category) { return UniquePointer.ArgResult(category); }
        internal Result PointerResult(Category category, Func<Category, Result> getCodeAndRefs)
        {
            return UniquePointer.Result(category, getCodeAndRefs);
        }

        internal Result Result(Category category, IContextReference target)
        {
            if(Hllw)
                return Result(category);
            return new Result
                (
                category,
                getType: () => this,
                getCode: () => CodeBase.ReferenceCode(target)
                );
        }

        internal Result Result(Category category, Result codeAndRefs)
        {
            return new Result
                (
                category,
                getType: () => this,
                getCode: () => codeAndRefs.Code,
                getExts: () => codeAndRefs.Exts
                );
        }

        internal Result Result(Category category, Func<Category, Result> getCodeAndRefs)
        {
            var localCategory = category & (Category.Code | Category.Exts);
            var codeAndRefs = getCodeAndRefs(localCategory);
            return Result
                (
                    category,
                    () => codeAndRefs.Code,
                    () => codeAndRefs.Exts
                );
        }

        internal Result Result(Category category, Func<CodeBase> getCode = null, Func<CodeArgs> getArgs = null)
        {
            return new Result
                (
                category,
                getType: () => this,
                getCode: getCode,
                getExts: getArgs
                );
        }

        internal TypeBase CommonType(TypeBase elseType) { return elseType.IsConvertable(this) ? this : elseType; }

        Result ConvertToSequence(Category category, TypeBase elementType)
        {
            return Conversion(category, CreateSequenceType(elementType));
        }

        Result ConvertToBitSequence(Category category)
        {
            return ConvertToSequence(category, BitType).Align(BitsConst.SegmentAlignBits);
        }

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value> The icon key. </value>
        string IIconKeyProvider.IconKey { get { return "Type"; } }

        [DisableDump]
        internal TypeType UniqueTypeType { get { return _cache.TypeType.Value; } }

        [DisableDump]
        internal TypeBase UniqueFunctionInstanceType { get { return _cache.FunctionInstanceType.Value; } }

        [DisableDump]
        internal virtual Structure FindRecentStructure
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal virtual bool IsLambda { get { return false; } }

        [DisableDump]
        internal virtual IReferenceType ReferenceType { get { return this as IReferenceType; } }

        [DisableDump]
        internal bool IsWeakReference { get { return ReferenceType != null && ReferenceType.IsWeak; } }

        [DisableDump]
        internal virtual IFeatureImplementation Feature { get { return this as IFeatureImplementation; } }

        [DisableDump]
        internal virtual bool HasQuickSize { get { return true; } }

        [DisableDump]
        internal VoidType VoidType { get { return RootContext.VoidType; } }

        [DisableDump]
        internal BitType BitType { get { return RootContext.BitType; } }

        [DisableDump]
        internal TypeBase TypeForConversion
        {
            get
            {
                return DePointer(Category.Type).Type
                    .DeAlign(Category.Type).Type;
            }
        }

        [DisableDump]
        internal virtual TypeBase CoreType { get { return this; } }

        [DisableDump]
        internal TypeBase TypeForSearchProbes
        {
            get
            {
                return DePointer(Category.Type).Type
                    .DeAlign(Category.Type).Type;
            }
        }

        [DisableDump]
        internal TypeBase TypeForStructureElement { get { return DeAlign(Category.Type).Type; } }

        [DisableDump]
        internal TypeBase TypeForArrayElement { get { return DeAlign(Category.Type).Type; } }

        [DisableDump]
        internal virtual TypeBase TypeForTypeOperator
        {
            get
            {
                return DeFunction(Category.Type).Type
                    .DePointer(Category.Type).Type
                    .DeAlign(Category.Type).Type;
            }
        }

        [DisableDump]
        internal virtual TypeBase ElementTypeForReference
        {
            get
            {
                return DeFunction(Category.Type).Type
                    .DePointer(Category.Type).Type
                    .DeAlign(Category.Type).Type;
            }
        }

        internal virtual Result DeAlign(Category category) { return ArgResult(category); }
        internal virtual ResultCache DeFunction(Category category) { return ArgResult(category); }
        internal virtual ResultCache DePointer(Category category) { return ArgResult(category); }

        TypeBase CreateSequenceType(TypeBase elementType)
        {
            return elementType
                .UniqueArray(SequenceLength(elementType))
                .UniqueSequence;
        }

        internal int SequenceLength(TypeBase elementType)
        {
            var length = SmartSequenceLength(elementType);
            if(length != null)
                return length.Value;

            NotImplementedMethod(elementType);
            return -1;
        }

        internal int ArrayLength(TypeBase elementType)
        {
            var length = SmartArrayLength(elementType);
            if(length != null)
                return length.Value;

            NotImplementedMethod(elementType);
            return -1;
        }

        internal Result LocalReferenceResult(Category category)
        {
            if(Hllw)
                return ArgResult(category);

            return UniquePointer
                .Result
                (
                    category,
                    LocalReferenceCode,
                    () => Destructor(Category.Exts).Exts + CodeArgs.Arg()
                );
        }

        CodeBase LocalReferenceCode()
        {
            return ArgCode
                .LocalReference(Destructor(Category.Code).Code);
        }

        internal Result ReferenceInCode(Category category, IContextReference target)
        {
            return UniquePointer
                .Result(category, target);
        }

        internal Result ContextAccessResult(Category category, IContextReference target, Func<Size> getOffset)
        {
            if(Hllw)
                return Result(category);
            return new Result
                (
                category,
                getType: () => this,
                getCode: () => CodeBase.ReferenceCode(target).ReferencePlus(getOffset()).DePointer(Size)
                );
        }

        IReferenceType ObtainPointer() { return this as IReferenceType ?? new PointerType(this); }

        [DisableDump]
        public virtual TypeBase ArrayElementType
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        protected virtual ArrayType ObtainArray(int count) { return new ArrayType(this, count); }

        internal CodeBase BitSequenceOperation(string token) { return UniqueAlign.ArgCode.NumberOperation(token, Size); }

        [NotNull]
        internal Result GenericDumpPrintResult(Category category)
        {
            return Declarations<DumpPrintToken>(null).Single().CallResult(category);
        }

        internal Result CreateArray(Category category)
        {
            return UniqueAlign
                .UniqueArray(1).UniquePointer
                .Result(category, PointerArgResult(category));
        }

        internal bool IsConvertable(TypeBase destination)
        {
            return TypeForConversion == destination.TypeForConversion
                || ConvertersForType(destination, ConversionParameter.Instance) != null;
        }

        internal Result Conversion(Category category, TypeBase destination)
        {
            if(category <= (Category.Type.Replenished))
                return destination.PointerKind.Result(category);

            var obviousConversionResult = ObviousConversion(category.Typed, destination);
            if(obviousConversionResult != null)
                return obviousConversionResult;

            var searchResults = TypeForConversion
                .ConvertersForType(destination.TypeForConversion, ConversionParameter.Instance)
                .ToArray();

            switch(searchResults.Length)
            {
                case 1:
                    return searchResults[0].ConversionResult(category, this, destination);
            }

            NotImplementedMethod(category, destination);
            return null;
        }

        internal Result ObviousConversion(Category category, TypeBase destination)
        {
            if(TypeForConversion != destination.TypeForConversion)
                return null;

            if(ReferenceType == null)
                return LocalReferenceResult(category);

            return ArgResult(category);
        }

        internal Result ObviousExactConversion(Category category, TypeBase destination)
        {
            if(CoreType == destination.CoreType)
                return destination.ArgResult(category);

            var sourcePointer = this as PointerType;
            if(sourcePointer == null)
                return UniquePointer
                    .ObviousExactConversion(category, destination)
                    .ReplaceArg(LocalReferenceResult(category));

            var destinationPointer = destination as PointerType;
            if(destinationPointer == null)
            {
                var pointer = destination.UniquePointer;
                return pointer.DePointer(category).Data
                    .ReplaceArg(ObviousExactConversion(category, pointer));
            }

            if(sourcePointer.ValueType.CoreType == destinationPointer.ValueType.CoreType)
                return destination.Result(category, () => ArgCode, CodeArgs.Arg);

            NotImplementedMethod(category, destination);
            return null;
        }

        internal Result TextItemResult(Category category)
        {
            var uniqueTextItem = UniqueTextItemType;
            return
                uniqueTextItem.UniquePointer
                    .Result(category, UniquePointer.ArgResult(category))
                ;
        }

        internal virtual Result ConstructorResult(Category category, TypeBase argsType)
        {
            return argsType.Conversion(category, this);
        }

        internal Result ConcatArrayFromReference(Category category, PointerType pointerType)
        {
            NotImplementedMethod(category, pointerType);
            return null;
        }

        Result DereferenceReferenceResult(Category category)
        {
            return UniquePointer
                .ArgResult(category.Typed).DereferenceResult;
        }

        internal Result BitSequenceOperandConversion(Category category)
        {
            return ConvertToBitSequence(category).AutomaticDereferenceResult
                .Align(BitsConst.SegmentAlignBits);
        }

        internal Result DumpPrintTypeNameResult(Category category)
        {
            return VoidType
                .Result
                (
                    category,
                    () => CodeBase.DumpPrintText(DumpPrintText),
                    CodeArgs.Void
                );
        }

        Simple Convert(Aligner type)
        {
            if(type.Parent == this)
                return Reni.Feature.Extension.SimpleFeature(PointerArgResult);
            return null;
        }

        protected Simple Convert(TypeBase type)
        {
            if(type.TypeForConversion == TypeForConversion)
                return Reni.Feature.Extension.SimpleFeature(DereferenceReferenceResult);

            return null;
        }

        internal TypeBase SmartUn<T>()
            where T : IConverter
        {
            return this is T ? ((IConverter) this).Result(Category.Type).Type : this;
        }

        internal Result PointerConversionResult(Category category, TypeBase destinationType)
        {
            return destinationType
                .UniquePointer
                .Result(category, UniquePointer.ArgResult(category.Typed));
        }

        internal virtual Result InstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            NotImplementedMethod(category, getRightResult(Category.All));
            return null;
        }

        internal CodeBase DumpPrintNumberCode()
        {
            var alignedSize = Size.Align(Root.DefaultRefAlignParam.AlignBits);
            return UniquePointer
                .ArgCode
                .DePointer(alignedSize)
                .DumpPrintNumber(alignedSize);
        }

        internal virtual IEnumerable<SearchResult> ConvertersForType(TypeBase destination, IConversionParameter parameter)
        {
            return destination.Genericize.SelectMany(g => g.Converters(this, parameter));
        }

        internal IEnumerable<SearchResult> ConvertersForType<TDestination>
            (TDestination destination, IConversionParameter parameter)
        {
            var provider = this as IConverterProvider<TDestination, IFeatureImplementation>;
            if(provider != null)
            {
                var feature = provider.Feature(destination, parameter);
                if(feature != null)
                    yield return new TypeSearchResult(feature, this);
            }
        }

        internal IEnumerable<SearchResult> DeclarationsForType(Definable tokenClass)
        {
            return tokenClass.Genericize.SelectMany(g=>g.Declarations(this));
        }

        internal IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass) where TDefinable : Definable
        {
            var provider = this as ISymbolProvider<TDefinable, IFeatureImplementation>;
            if(provider != null)
                yield return new TypeSearchResult(provider.Feature(tokenClass), this);
            var inheritor = this as IFeatureInheritor;
            if(inheritor != null)
                yield return inheritor.ResolveDeclarations(tokenClass);
        }

        [DisableDump]
        protected virtual IEnumerable<IGenericProviderForType> Genericize { get { return this.GenericListFromType(); } }
    }

    // Krautpuster
    // Gurkennudler
}