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
            public readonly FunctionCache<int, FunctionCache<bool, ArrayType>> Array;
            [Node]
            [SmartNode]
            public readonly FunctionCache<TypeBase, Pair> Pair;
            [Node]
            [SmartNode]
            public readonly ValueCache<IReference> Reference;
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
                Reference = new ValueCache<IReference>(parent.ObtainReference);
                Pair = new FunctionCache<TypeBase, Pair>(first => new Pair(first, parent));
                Array = new FunctionCache<int, FunctionCache<bool, ArrayType>>(
                    count => new FunctionCache<bool, ArrayType>(isMutable=>parent.ObtainArray(count, isMutable)));
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
            : base(_nextObjectId++) { _cache = new Cache(this); }

        IContextReference IContextReferenceProvider.ContextReference => Reference;

        [Node]
        internal Size Size => _cache.Size.Value;

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
        internal virtual TypeBase[] ToList => new[] {this};


        [DisableDump]
        internal virtual string DumpPrintText => NodeDump;

        internal virtual int? SmartSequenceLength(TypeBase elementType) => SmartArrayLength(elementType);

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
        internal TypeBase Pointer => Reference.Type();

        [DisableDump]
        internal virtual IReference Reference => _cache.Reference.Value;

        [DisableDump]
        internal CodeBase ArgCode => CodeBase.Arg(this);

        [DisableDump]
        internal TypeBase AutomaticDereferenceType
            => IsWeakReference ? ReferenceType.Converter.TargetType.AutomaticDereferenceType : this;

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
        internal virtual Size SimpleItemSize => null;

        Result VoidCodeAndRefs(Category category) => RootContext.VoidResult(category & (Category.Code | Category.Exts));

        internal ArrayType Array(int count, bool isMutable) => _cache.Array[count][isMutable];
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

        internal Result Result(Category category, Result codeAndRefs) => new Result
            (
            category,
            getType: () => this,
            getCode: () => codeAndRefs.Code,
            getExts: () => codeAndRefs.Exts
            );

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
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual IReference ReferenceType => this as IReference;

        [DisableDump]
        internal bool IsWeakReference => ReferenceType != null && ReferenceType.IsWeak;

        [DisableDump]
        internal virtual IFeatureImplementation Feature => this as IFeatureImplementation;

        [DisableDump]
        internal virtual bool HasQuickSize => true;

        [DisableDump]
        internal VoidType VoidType => RootContext.VoidType;

        [DisableDump]
        internal BitType BitType => RootContext.BitType;

        [DisableDump]
        internal virtual TypeBase CoreType => this;

        [DisableDump]
        internal TypeBase TypeForSearchProbes => DePointer(Category.Type).Type
            .DeAlign(Category.Type).Type;

        [DisableDump]
        internal TypeBase TypeForStructureElement => DeAlign(Category.Type).Type;

        [DisableDump]
        internal TypeBase TypeForArrayElement => DeAlign(Category.Type).Type;

        [DisableDump]
        internal virtual TypeBase TypeForTypeOperator => DeFunction(Category.Type).Type
            .DePointer(Category.Type).Type
            .DeAlign(Category.Type).Type;

        [DisableDump]
        internal virtual TypeBase ElementTypeForReference => DeFunction(Category.Type).Type
            .DePointer(Category.Type).Type
            .DeAlign(Category.Type).Type;

        protected virtual Result DeAlign(Category category) => ArgResult(category);
        protected virtual ResultCache DeFunction(Category category) => ArgResult(category);
        internal virtual ResultCache DePointer(Category category) => ArgResult(category);

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

            return Pointer
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

        internal Result ReferenceInCode(Category category, IContextReference target) => Pointer
            .Result(category, target);

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

        IReference ObtainReference()
        {
            Tracer.Assert(!Hllw);
            return this as IReference ?? new PointerType(this);
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

        protected virtual ArrayType ObtainArray(int count, bool isMutable) => new ArrayType(this, count, isMutable);

        internal CodeBase BitSequenceOperation(string token) => Align.ArgCode.NumberOperation(token, Size);

        [NotNull]
        internal Result GenericDumpPrintResult(Category category)
            => Declarations<DumpPrintToken>(null).Single().CallResult(category);

        internal Result CreateArray(Category category, bool isMutable) => Align
            .Array(1, isMutable).Pointer
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

        internal virtual Result ConstructorResult(Category category, TypeBase argsType) => argsType.Conversion(category, this);

        internal Result DumpPrintTypeNameResult(Category category) => VoidType
            .Result
            (
                category,
                () => CodeBase.DumpPrintText(DumpPrintText),
                CodeArgs.Void
            );

        internal TypeBase SmartUn<T>()
            where T : ISimpleFeature => this is T ? ((ISimpleFeature) this).Result(Category.Type).Type : this;

        internal Result PointerConversionResult(Category category, TypeBase destinationType) => destinationType
            .Pointer
            .Result(category, Pointer.ArgResult(category.Typed));

        internal virtual Result InstanceResult(Category category, Func<Category, Result> getRightResult)
        {
            NotImplementedMethod(category, getRightResult(Category.All));
            return null;
        }

        internal IEnumerable<SearchResult> DeclarationsForType(Definable tokenClass)
            => tokenClass.Genericize.SelectMany(g => g.Declarations(this));

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
        protected virtual IEnumerable<IGenericProviderForType> Genericize => this.GenericListFromType();
        [DisableDump]
        [NotNull]
        public IEnumerable<ISimpleFeature> SymmetricConversions => _cache.SymmetricConversions.Value;

        Result AlignResult(Category category) { return Align.Result(category, () => ArgCode.Align(), CodeArgs.Arg); }

        IEnumerable<ISimpleFeature> ObtainSymmetricConversions()
            => ObtainRawSymmetricConversions()
                .ToDictionary(x => x.ResultType())
                .Values;

        protected virtual IEnumerable<ISimpleFeature> ObtainRawSymmetricConversions()
        {
            if(Hllw)
                yield break;
            if(IsAligningPossible && Align.Size != Size)
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
            Tracer.Assert(result.Count() <= 1);

            return result;
        }
        internal IEnumerable<ISimpleFeature> GetForcedConversions<TDestination>()
        {
            NotImplementedMethod();
            return null;
        }

        internal IEnumerable<ISimpleFeature> GetForcedConversions<TDestination>(TDestination destination)
        {
            var provider = this as IForcedConversionProvider<TDestination>;
            if(provider != null)
                return provider.Result(destination);
            return new ISimpleFeature[0];
        }

        [DisableDump]
        internal virtual IEnumerable<ISimpleFeature> StripConversions { get { yield break; } }
        internal virtual IEnumerable<ISimpleFeature> CutEnabledConversion(NumberType destination) { yield break; }

        protected Result DumpPrintTokenResult(Category category) => VoidType.Result(category, DumpPrintCode, CodeArgs.Arg);

        protected virtual CodeBase DumpPrintCode()
        {
            NotImplementedMethod();
            return null;
        }
    }

    interface IForcedConversionProvider<in TDestination>
    {
        IEnumerable<ISimpleFeature> Result(TDestination destination);
    }

    // Krautpuster
    // Gurkennudler
}