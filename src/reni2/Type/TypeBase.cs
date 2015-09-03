using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
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
            public readonly ValueCache<IEnumerable<IValue>> SymmetricConversions;
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

                Aligner = new FunctionCache<int, AlignType>
                    (alignBits => new AlignType(parent, alignBits));
                FunctionInstanceType = new ValueCache<FunctionInstanceType>
                    (() => new FunctionInstanceType(parent));
                TypeType = new ValueCache<TypeType>(() => new TypeType(parent));
                Size = new ValueCache<Size>(parent.GetSizeForCache);
                SymmetricConversions = new ValueCache<IEnumerable<IValue>>
                    (parent.GetSymmetricConversionsForCache);
                ArrayReferenceCache = new FunctionCache<string, ArrayReferenceType>
                    (id => new ArrayReferenceType(parent, id));
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
            =>
                IsWeakReference
                    ? CheckedReference.Converter.Source.AutomaticDereferenceType
                    : this;

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

        Result VoidCodeAndRefs(Category category)
            => RootContext.VoidType.Result(category & (Category.Code | Category.Exts));

        internal ArrayType Array(int count, string options = null)
            => _cache.Array[count][options ?? ArrayType.Options.DefaultOptionsId];

        internal ArrayReferenceType ArrayReference(string optionsId)
            => _cache.ArrayReferenceCache[optionsId];

        protected virtual TypeBase ReversePair(TypeBase first) => first._cache.Pair[this];
        internal virtual TypeBase Pair(TypeBase second) => second.ReversePair(this);
        internal virtual Result Destructor(Category category) => VoidCodeAndRefs(category);

        internal virtual Result ArrayDestructor(Category category)
            => VoidCodeAndRefs(category);

        internal virtual Result Copier(Category category) => VoidCodeAndRefs(category);

        internal virtual Result ArrayCopier(Category category)
            => VoidCodeAndRefs(category);

        internal virtual Result ApplyTypeOperator(Result argResult)
            => argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult);

        internal Result ArgResult(Category category)
        {
            return Result(category, () => ArgCode, CodeArgs.Arg);
        }

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

        internal Result Result
            (Category category, Func<CodeBase> getCode = null, Func<CodeArgs> getArgs = null)
            => new Result
                (
                category,
                getType: () => this,
                getCode: getCode,
                getExts: getArgs
                );

        internal TypeBase CommonType(TypeBase elseType)
            => elseType.IsConvertable(this) ? this : elseType;

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
        internal virtual IImplementation CheckedFeature => this as IImplementation;

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
            => DePointer(Category.Type).Type.DeAlign(Category.Type).Type;

        [DisableDump]
        internal virtual TypeBase ElementTypeForReference
            => DePointer(Category.Type).Type
                .DeAlign(Category.Type).Type;

        protected virtual Result DeAlign(Category category) => ArgResult(category);
        protected virtual ResultCache DePointer(Category category) => ArgResult(category);

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

        internal Result ContextAccessResult
            (Category category, IContextReference target, Func<Size> getOffset)
        {
            if(Hllw)
                return Result(category);
            return new Result
                (
                category,
                getType: () => this,
                getCode:
                    () => CodeBase.ReferenceCode(target).ReferencePlus(getOffset()).DePointer(Size)
                );
        }

        IReference ForcedReferenceForCache()
        {
            Tracer.Assert(!Hllw);
            return CheckedReference ?? ForcedPointer;
        }

        protected virtual PointerType ForcedPointerForCache()
        {
            Tracer.Assert(!Hllw);
            return new PointerType(this);
        }


        protected virtual ArrayType ArrayForCache(int count, string optionsId)
            => new ArrayType(this, count, optionsId);

        [NotNull]
        internal Result GenericDumpPrintResult(Category category)
            => SmartPointer
                .Declarations<DumpPrintToken>(null)
                .Single()
                .SpecialExecute(category);

        internal Result CreateArray(Category category, string optionsId = null) => Align
            .Array(1, optionsId).Pointer
            .Result(category, PointerArgResult(category));

        internal bool IsConvertable(TypeBase destination)
            => ConversionService.FindPath(this, destination) != null;

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
            StartMethodDump(false, category, argsType);
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
            where T : IValue
            => this is T ? ((IValue) this).Result(Category.Type).Type : this;

        internal Result ResultFromPointer(Category category, TypeBase resultType) => resultType
            .Pointer
            .Result(category, ObjectResult);

        internal virtual Result InstanceResult
            (Category category, Func<Category, Result> getRightResult)
        {
            NotImplementedMethod(category, getRightResult(Category.All));
            return null;
        }

        [DisableDump]
        IEnumerable<SearchResult> FuncionDeclarationsForType
        {
            get
            {
                var result = FuncionDeclarationForType;
                if(result != null)
                    yield return SearchResult.Create(result, this);
            }
        }

        [DisableDump]
        internal virtual IImplementation FuncionDeclarationForType => null;
        [DisableDump]
        internal virtual IImplementation FunctionDeclarationForPointerType => null;

        /// <summary>
        ///     Call this function to get declarations of definable for this type.
        /// </summary>
        /// <param name="definable"></param>
        /// <returns></returns>
        internal IEnumerable<SearchResult> DeclarationsForType(Definable definable)
        {
            if(definable == null)
                return FuncionDeclarationsForType;

            return definable
                .Genericize
                .SelectMany(g => g.Declarations(this))
                .ToArray();
        }

        /// <summary>
        ///     Call this function to get declarations of definable for this type
        ///     and its close relatives (see <see cref="ConversionService.CloseRelativeConversions" />).
        /// </summary>
        /// <param name="tokenClass"></param>
        /// <returns></returns>
        IEnumerable<SearchResult> DeclarationsForTypeAndCloseRelatives
            (Definable tokenClass)
        {
            var result = DeclarationsForType(tokenClass).ToArray();
            if(result.Any())
                return result;

            var closeRelativeConversions = this
                .CloseRelativeConversions()
                .ToArray();
            return closeRelativeConversions
                .SelectMany(path => path.CloseRelativeSearchResults(tokenClass))
                .ToArray();
        }

        /// <summary>
        ///     Override this function to provide declarations of a definable for this type.
        ///     Only declaration, that are made exactly for type <see cref="TDefinable" />
        ///     should be considered.
        ///     This implementation checks if this type is symbol provider for definable.
        ///     Dont call this except in overriden versions.
        /// </summary>
        /// <typeparam name="TDefinable"></typeparam>
        /// <param name="tokenClass"></param>
        /// <returns></returns>
        internal virtual IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass)
            where TDefinable : Definable
        {
            var provider = this as ISymbolProvider<TDefinable>;
            if(provider == null)
                yield break;
            var feature = provider.Feature(tokenClass);
            if(feature != null)
                yield return SearchResult.Create(feature, this);
        }

        [DisableDump]
        protected virtual IEnumerable<IGenericProviderForType> Genericize
            => this.GenericListFromType();
        [DisableDump]
        [NotNull]
        public IEnumerable<IValue> SymmetricConversions => _cache.SymmetricConversions.Value
            ;

        Result AlignResult(Category category)
        {
            return Align.Result(category, () => ArgCode.Align(), CodeArgs.Arg);
        }

        IEnumerable<IValue> GetSymmetricConversionsForCache()
            => RawSymmetricConversions
                .ToDictionary(x => x.ResultType())
                .Values;

        [DisableDump]
        protected virtual IEnumerable<IValue> RawSymmetricConversions
        {
            get
            {
                if(Hllw)
                    yield break;
                if(IsAligningPossible && Align.Size != Size)
                    yield return Feature.Extension.Value(AlignResult);
                if(IsPointerPossible)
                    yield return Feature.Extension.Value(LocalReferenceResult);
            }
        }

        internal IEnumerable<IValue> GetForcedConversions(TypeBase destination)
        {
            var genericProviderForTypes = destination
                .Genericize
                .ToArray();
            var result = genericProviderForTypes
                .SelectMany(g => g.GetForcedConversions(this).ToArray())
                .ToArray();

            Tracer.Assert(result.All(f => f.Source == this));
            Tracer.Assert(result.All(f => f.ResultType() == destination));
            Tracer.Assert(result.Length <= 1);

            return result;
        }

        internal virtual IEnumerable<IValue> GetForcedConversions<TDestination>
            (TDestination destination)
        {
            var provider = this as IForcedConversionProvider<TDestination>;
            if(provider != null)
                return provider.Result(destination);
            return new IValue[0];
        }

        [DisableDump]
        internal virtual IEnumerable<IValue> StripConversions { get { yield break; } }


        internal virtual IEnumerable<IValue> CutEnabledConversion(NumberType destination)
        {
            yield break;
        }

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

        internal Result IssueResult(SourcePart source, IssueId issueId, Category category)
            => CreateIssue(source, issueId).Result(category);

        protected virtual IssueType CreateIssue(SourcePart source, IssueId issueId)
            => new RootIssueType
                (
                new Issue(issueId, source, "Type: " + DumpPrintText),
                RootContext
                );

        internal Result Execute
            (
            Category category,
            ResultCache left,
            SourcePart token,
            Definable definable,
            ContextBase context,
            CompileSyntax right
            )
            => ExecuteDeclaration
                (
                    definable,
                    result => result.Execute(category, left, context, right, token),
                    issueId => IssueResult(token, issueId, category)
                );

        internal TResult ExecuteDeclaration<TResult>
            (Definable definable, Func<SearchResult, TResult> execute, Func<IssueId, TResult> onError)
        {
            var searchResults
                = DeclarationsForTypeAndCloseRelatives(definable)
                    .RemoveLowPriorityResults()
                    .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    return onError(IssueId.UndefinedSymbol);
                case 1:
                    return execute(searchResults.First());
                default:
                    return onError(IssueId.AmbigousSymbol);
            }
        }

        [DisableDump]
        internal virtual IEnumerable<Syntax> Mixins
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal virtual ContextBase ToContext
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
    }


    interface IForcedConversionProvider<in TDestination>
    {
        IEnumerable<IValue> Result(TDestination destination);
    }

    interface IForcedConversionProviderForPointer<in TDestination>
    {
        IEnumerable<IValue> Result(TDestination destination);
    }

    // Krautpuster
    // Gurkennudler
}