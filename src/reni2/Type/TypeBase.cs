using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Validation;

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
            public readonly FunctionCache<int, AlignType> Aligner;
            [Node]
            [SmartNode]
            public readonly FunctionCache<int, FunctionCache<string, ArrayType>> Array;
            [Node]
            [SmartNode]
            public readonly FunctionCache<TypeBase, Pair> Pair;
            [Node]
            [SmartNode]
            public readonly ValueCache<IReference> ForcedReference;
            [Node]
            [SmartNode]
            public readonly ValueCache<PointerType> ForcedPointer;
            [Node]
            [SmartNode]
            public readonly ValueCache<TypeType> TypeType;
            [Node]
            [SmartNode]
            public readonly ValueCache<FunctionInstanceType> FunctionInstanceType;
            [Node]
            [SmartNode]
            public readonly ValueCache<EnableCut> EnableCut;
            public readonly ValueCache<Size> Size;
            [Node]
            [SmartNode]
            public readonly ValueCache<IEnumerable<ISimpleFeature>> SymmetricConversions;
            [Node]
            [SmartNode]
            internal readonly FunctionCache<string, ArrayReferenceType> ArrayReferenceCache;

            public Cache(TypeBase parent)
            {
                EnableCut = new ValueCache<EnableCut>(() => new EnableCut(parent));
                ForcedReference = new ValueCache<IReference>(parent.ForcedReferenceForCache);
                ForcedPointer = new ValueCache<PointerType>(parent.ForcedPointerForCache);
                Pair = new FunctionCache<TypeBase, Pair>(first => new Pair(first, parent));
                Array = new FunctionCache<int, FunctionCache<string, ArrayType>>
                    (
                    count
                        =>
                        new FunctionCache<string, ArrayType>
                            (
                            optionsId
                                =>
                                parent.ArrayForCache(count, optionsId)
                            )
                    );

                Aligner = new FunctionCache<int, AlignType>(alignBits => new AlignType(parent, alignBits));
                FunctionInstanceType = new ValueCache<FunctionInstanceType>(() => new FunctionInstanceType(parent));
                TypeType = new ValueCache<TypeType>(() => new TypeType(parent));
                Size = new ValueCache<Size>(parent.GetSizeForCache);
                SymmetricConversions = new ValueCache<IEnumerable<ISimpleFeature>>(parent.GetSymmetricConversionsForCache);
                ArrayReferenceCache = new FunctionCache<string, ArrayReferenceType>(id => new ArrayReferenceType(parent, id));
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

        IContextReference IContextReferenceProvider.ContextReference => ForcedReference;

        [Node]
        internal Size Size => _cache.Size.Value;

        [NotNull]
        protected virtual Size GetSize()
        {
            NotImplementedMethod();
            return Size.Zero;
        }

        [NotNull]
        Size GetSizeForCache()
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
        internal virtual TypeBase[] ToList => new[] {this};


        [DisableDump]
        internal virtual string DumpPrintText => NodeDump;

        internal virtual int? SmartArrayLength(TypeBase elementType)
        {
            if(IsConvertable(elementType))
                return 1;
            NotImplementedMethod(elementType);
            return null;
        }

        [DisableDump]
        internal EnableCut EnableCut => _cache.EnableCut.Value;

        [DisableDump]
        internal TypeBase Pointer => ForcedReference.Type();

        [DisableDump]
        internal virtual IReference ForcedReference => _cache.ForcedReference.Value;

        [DisableDump]
        internal CodeBase ArgCode => CodeBase.Arg(this);

        [DisableDump]
        internal TypeBase AutomaticDereferenceType
            => IsWeakReference ? CheckedReference.Converter.TargetType.AutomaticDereferenceType : this;

        [DisableDump]
        internal TypeBase SmartPointer => Hllw ? this : Pointer;

        [DisableDump]
        internal TypeBase Align
        {
            get
            {
                var alignBits = Root.DefaultRefAlignParam.AlignBits;
                if(Size.Align(alignBits) == Size)
                    return this;
                return _cache.Aligner[alignBits];
            }
        }

        [DisableDump]
        internal virtual bool IsCuttingPossible => false;

        [DisableDump]
        internal virtual bool IsAligningPossible => true;

        [DisableDump]
        internal virtual bool IsPointerPossible => true;
        [DisableDump]
        internal virtual Size SimpleItemSize => null;

        Result VoidCodeAndRefs(Category category) => RootContext.VoidType.Result(category & (Category.Code | Category.Exts));

        internal ArrayType Array(int count, string options = null)
            => _cache.Array[count][options ?? ArrayType.Options.DefaultOptionsId];
        internal ArrayReferenceType ArrayReference(string optionsId) => _cache.ArrayReferenceCache[optionsId];
        protected virtual TypeBase ReversePair(TypeBase first) => first._cache.Pair[this];
        internal virtual TypeBase Pair(TypeBase second) => second.ReversePair(this);
        internal virtual Result Destructor(Category category) => VoidCodeAndRefs(category);
        internal virtual Result ArrayDestructor(Category category, int count) => VoidCodeAndRefs(category);
        internal virtual Result Copier(Category category) => VoidCodeAndRefs(category);
        internal virtual Result ArrayCopier(Category category, int count) => VoidCodeAndRefs(category);
        internal virtual Result ApplyTypeOperator(Result argResult)
            => argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult);
        internal Result ArgResult(Category category) { return Result(category, () => ArgCode, CodeArgs.Arg); }
        Result PointerArgResult(Category category) => Pointer.ArgResult(category);

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

        internal Result Result(Category category, Result codeAndExts) => new Result
            (
            category,
            getType: () => this,
            getCode: () => codeAndExts.Code,
            getExts: () => codeAndExts.Exts
            );

        internal Result Result(Category category, Func<Category, Result> getCodeAndRefs)
        {
            var localCategory = category & (Category.Code | Category.Exts);
            var codeAndExts = getCodeAndRefs(localCategory);
            return Result
                (
                    category,
                    () => codeAndExts.Code,
                    () => codeAndExts.Exts
                );
        }

        internal Result Result(Category category, Func<CodeBase> getCode = null, Func<CodeArgs> getArgs = null) => new Result
            (
            category,
            getType: () => this,
            getCode: getCode,
            getExts: getArgs
            );

        internal TypeBase CommonType(TypeBase elseType) => elseType.IsConvertable(this) ? this : elseType;

        /// <summary>
        ///     Gets the icon key.
        /// </summary>
        /// <value> The icon key. </value>
        string IIconKeyProvider.IconKey => "Type";

        [DisableDump]
        internal TypeType TypeType => _cache.TypeType.Value;

        [DisableDump]
        internal TypeBase FunctionInstance => _cache.FunctionInstanceType.Value;

        internal PointerType ForcedPointer => _cache.ForcedPointer.Value;
        [DisableDump]
        internal virtual CompoundView FindRecentCompoundView
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal virtual IReference CheckedReference => this as IReference;

        [DisableDump]
        internal bool IsWeakReference => CheckedReference != null && CheckedReference.IsWeak;

        [DisableDump]
        internal virtual IFeatureImplementation CheckedFeature => this as IFeatureImplementation;

        [DisableDump]
        internal virtual bool HasQuickSize => true;

        [DisableDump]
        internal VoidType VoidType => RootContext.VoidType;

        [DisableDump]
        internal BitType BitType => RootContext.BitType;

        [DisableDump]
        internal virtual TypeBase CoreType => this;

        [DisableDump]
        internal TypeBase TypeForStructureElement => DeAlign(Category.Type).Type;

        [DisableDump]
        internal TypeBase TypeForArrayElement => DeAlign(Category.Type).Type;

        [DisableDump]
        internal virtual TypeBase TypeForTypeOperator
            => DePointer(Category.Type).Type
                .DeAlign(Category.Type).Type;

        [DisableDump]
        internal virtual TypeBase ElementTypeForReference
            => DePointer(Category.Type).Type
                .DeAlign(Category.Type).Type;

        protected virtual Result DeAlign(Category category) => ArgResult(category);
        internal virtual ResultCache DePointer(Category category) => ArgResult(category);

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

            return ForcedPointer
                .Result
                (
                    category,
                    LocalReferenceCode,
                    () => Destructor(Category.Exts).Exts + CodeArgs.Arg()
                );
        }

        CodeBase LocalReferenceCode()
            => ArgCode
                .LocalReference(this, Destructor(Category.Code).Code);

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

        IReference ForcedReferenceForCache()
        {
            Tracer.Assert(!Hllw);
            return CheckedReference ?? ForcedPointer;
        }

        internal virtual PointerType ForcedPointerForCache()
        {
            Tracer.Assert(!Hllw);
            return new PointerType(this);
        }


        protected virtual ArrayType ArrayForCache(int count, string optionsId)
            => new ArrayType(this, count, optionsId);

        [NotNull]
        internal Result GenericDumpPrintResult(Category category)
        {
            var searchResults = SmartPointer.Declarations<DumpPrintToken>(null);
            return searchResults.Single().CallResult(category);
        }

        internal Result CreateArray(Category category, string optionsId = null) => Align
            .Array(1, optionsId).Pointer
            .Result(category, PointerArgResult(category));

        internal bool IsConvertable(TypeBase destination) => ConversionService.FindPath(this, destination) != null;

        internal Result Conversion(Category category, TypeBase destination)
        {
            if(category <= (Category.Type.Replenished))
                return destination.SmartPointer.Result(category);

            var path = ConversionService.FindPath(this, destination);
            if(path != null)
                return path.Execute(category.Typed);

            NotImplementedMethod(category, destination);
            return null;
        }

        internal virtual Result ConstructorResult(Category category, TypeBase argsType)
        {
            StartMethodDump(false,category,argsType);
            try
            {
                BreakExecution();
                var result = argsType.Conversion(category, this);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result DumpPrintTypeNameResult(Category category) => VoidType
            .Result
            (
                category,
                () => CodeBase.DumpPrintText(DumpPrintText),
                CodeArgs.Void
            );

        internal TypeBase SmartUn<T>()
            where T : ISimpleFeature => this is T ? ((ISimpleFeature) this).Result(Category.Type).Type : this;

        internal Result ResultFromPointer(Category category, TypeBase resultType) => resultType
            .Pointer
            .Result(category, ObjectResult);

        internal virtual Result InstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            NotImplementedMethod(category, getRightResult(Category.All));
            return null;
        }

        internal IEnumerable<SearchResult> DeclarationsForType(Definable definable)
            => definable.Genericize.SelectMany(g => g.Declarations(this));

        internal IEnumerable<SearchResult> DeclarationsForTypeAndCloseRelatives(Definable tokenClass)
        {
            var result = DeclarationsForType(tokenClass).ToArray();
            if(result.Any())
                return result;

            var closeRelativeConversions = this
                .CloseRelativeConversions()
                .ToArray();
            return closeRelativeConversions
                .SelectMany(path => path.CloseRelativeSearchResults(tokenClass).ToArray())
                .ToArray();
        }

        internal virtual IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass) where TDefinable : Definable
        {
            var feature = (this as ISymbolProvider<TDefinable, IFeatureImplementation>)
                ?.Feature(tokenClass);
            return feature.NullableToArray().Select(f => new SearchResult(f, this));
        }

        [DisableDump]
        protected virtual IEnumerable<IGenericProviderForType> Genericize => this.GenericListFromType();
        [DisableDump]
        [NotNull]
        public IEnumerable<ISimpleFeature> SymmetricConversions => _cache.SymmetricConversions.Value;

        Result AlignResult(Category category) { return Align.Result(category, () => ArgCode.Align(), CodeArgs.Arg); }

        IEnumerable<ISimpleFeature> GetSymmetricConversionsForCache()
            => RawSymmetricConversions
                .ToDictionary(x => x.ResultType())
                .Values;

        [DisableDump]
        protected virtual IEnumerable<ISimpleFeature> RawSymmetricConversions
        {
            get
            {
                if(Hllw)
                    yield break;
                if(IsAligningPossible && Align.Size != Size)
                    yield return Feature.Extension.SimpleFeature(AlignResult);
                if(IsPointerPossible)
                    yield return Feature.Extension.SimpleFeature(LocalReferenceResult);
            }
        }

        internal IEnumerable<ISimpleFeature> GetForcedConversions(TypeBase destination)
        {
            var genericProviderForTypes = destination
                .Genericize
                .ToArray();
            var result = genericProviderForTypes
                .SelectMany(g => g.GetForcedConversions(this).ToArray())
                .ToArray();

            Tracer.Assert(result.All(f => f.TargetType == this));
            Tracer.Assert(result.All(f => f.ResultType() == destination));
            Tracer.Assert(result.Count() <= 1);

            return result;
        }

        internal virtual IEnumerable<ISimpleFeature> GetForcedConversions<TDestination>(TDestination destination)
        {
            var provider = this as IForcedConversionProvider<TDestination>;
            if(provider != null)
                return provider.Result(destination);
            return new ISimpleFeature[0];
        }

        [DisableDump]
        internal virtual IEnumerable<ISimpleFeature> StripConversions { get { yield break; } }
        internal virtual IEnumerable<ISimpleFeature> CutEnabledConversion(NumberType destination) { yield break; }

        protected Result DumpPrintTokenResult(Category category)
            => VoidType
                .Result(category, DumpPrintCode, CodeArgs.Arg)
                .ReplaceArg(ObjectResult(category).DereferenceResult);

        internal Result ObjectResult(Category category)
            => Hllw ? Result(category) : Pointer.Result(category.Typed, ForcedReference);

        protected virtual CodeBase DumpPrintCode()
        {
            NotImplementedMethod();
            return null;
        }
        internal IssueType UndefinedSymbol(ExpressionSyntax source)
            =>
                new IssueType
                    (
                    new Issue(IssueId.UndefinedSymbol, source, "Type: " + DumpPrintText),
                    RootContext);
    }


    interface IForcedConversionProvider<in TDestination>
    {
        IEnumerable<ISimpleFeature> Result(TDestination destination);
    }

    interface IForcedConversionProviderForPointer<in TDestination>
    {
        IEnumerable<ISimpleFeature> Result(TDestination destination);
    }

    // Krautpuster
    // Gurkennudler
}