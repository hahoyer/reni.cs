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
            public readonly FunctionCache<int, AlignType> Aligner;
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
            public readonly ValueCache<EnableCut> EnableCut;
            public readonly ValueCache<Size> Size;
            [Node]
            [SmartNode]
            public readonly ValueCache<IEnumerable<ISimpleFeature>> SymmetricConversions;

            public Cache(TypeBase parent)
            {
                EnableCut = new ValueCache<EnableCut>(() => new EnableCut(parent));
                Pointer = new ValueCache<IReferenceType>(parent.ObtainPointer);
                Pair = new FunctionCache<TypeBase, Pair>(first => new Pair(first, parent));
                Array = new FunctionCache<int, ArrayType>(parent.ObtainArray);
                Aligner = new FunctionCache<int, AlignType>(alignBits => new AlignType(parent, alignBits));
                FunctionInstanceType = new ValueCache<FunctionInstanceType>(() => new FunctionInstanceType(parent));
                TypeType = new ValueCache<TypeType>(() => new TypeType(parent));
                Size = new ValueCache<Size>(parent.ObtainSize);
                SymmetricConversions = new ValueCache<IEnumerable<ISimpleFeature>>(parent.ObtainSymmetricConversions);
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

        [DisableDump]
        internal virtual bool IsCuttingPossible { get { return false; } }

        [DisableDump]
        internal virtual bool IsAligningPossible { get { return true; } }

        [DisableDump]
        internal virtual Size SimpleItemSize { get { return null; } }

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
        Result PointerArgResult(Category category) { return UniquePointer.ArgResult(category); }

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
        internal virtual CompoundView FindRecentCompoundView
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

        protected virtual Result DeAlign(Category category) { return ArgResult(category); }
        protected virtual ResultCache DeFunction(Category category) { return ArgResult(category); }
        internal virtual ResultCache DePointer(Category category) { return ArgResult(category); }

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

        IReferenceType ObtainPointer()
        {
            Tracer.Assert(!Hllw);
            return this as IReferenceType ?? new PointerType(this);
        }

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

        internal bool IsConvertable(TypeBase destination) { return ConversionService.FindPath(this, destination) != null; }

        internal Result Conversion(Category category, TypeBase destination)
        {
            if(category <= (Category.Type.Replenished))
                return destination.PointerKind.Result(category);

            var path = ConversionService.FindPath(this, destination);
            if(path != null)
                return path.Execute(category);

            var reachable = ConversionService.DumpObvious(this);
            NotImplementedMethod(category, destination, "reachable", reachable);
            return null;
        }

        internal virtual Result ConstructorResult(Category category, TypeBase argsType)
        {
            return argsType.Conversion(category, this);
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

        internal TypeBase SmartUn<T>()
            where T : ISimpleFeature
        {
            return this is T ? ((ISimpleFeature) this).Result(Category.Type).Type : this;
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

        internal IEnumerable<SearchResult> DeclarationsForType(Definable tokenClass)
        {
            return tokenClass.Genericize.SelectMany(g => g.Declarations(this));
        }

        internal IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass) where TDefinable : Definable
        {
            var feature = (this as ISymbolProvider<TDefinable, IFeatureImplementation>)
                ?.Feature(tokenClass);
            if(feature != null)
                return new[] {new TypeSearchResult(feature, this)};

            var inheritor = this as IFeatureInheritor;
            if(inheritor != null)
                return inheritor.ResolveDeclarations(tokenClass);

            return new SearchResult[0];
        }

        [DisableDump]
        protected virtual IEnumerable<IGenericProviderForType> Genericize { get { return this.GenericListFromType(); } }
        [DisableDump]
        [NotNull]
        public IEnumerable<ISimpleFeature> SymmetricConversions { get { return _cache.SymmetricConversions.Value; } }

        Result AlignResult(Category category) { return UniqueAlign.Result(category, () => ArgCode.Align(), CodeArgs.Arg); }

        IEnumerable<ISimpleFeature> ObtainSymmetricConversions()
            => ObtainRawSymmetricConversions()
                .ToDictionary(x => x.ResultType())
                .Values;

        protected virtual IEnumerable<ISimpleFeature> ObtainRawSymmetricConversions()
        {
            if(Hllw)
                yield break;
            if(IsAligningPossible)
                yield return Reni.Feature.Extension.SimpleFeature(AlignResult);
            if(!(this is PointerType))
                yield return Reni.Feature.Extension.SimpleFeature(LocalReferenceResult);
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

            return result;
        }

        internal IEnumerable<ISimpleFeature> GetForcedConversions<TDestination>(TDestination destination)
        {
            var provider = this as IForcedConversionProvider<TDestination>;
            if(provider != null)
                return provider.Result(destination);
            return new ISimpleFeature[0];
        }

        internal virtual IEnumerable<ISimpleFeature> StripConversions { get { yield break; } }
        internal virtual IEnumerable<ISimpleFeature> CutEnabledConversion(NumberType destination) { yield break; }
    }

    interface IForcedConversionProvider<in TDestination>
    {
        IEnumerable<ISimpleFeature> Result(TDestination destination);
    }

    // Krautpuster
    // Gurkennudler
}