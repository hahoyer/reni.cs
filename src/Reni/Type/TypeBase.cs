using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Type;

abstract class TypeBase
    : DumpableObject
        , IContextReferenceProvider
        , IIconKeyProvider
        , ISearchTarget
        , ValueCache.IContainer
        , IRootProvider
        , ISymbolProviderForPointer<IdentityOperation>

{
    sealed class CacheContainer
    {
        [Node]
        [SmartNode]
        public readonly FunctionCache<int, AlignType> Aligner;

        [Node]
        [SmartNode]
        public readonly FunctionCache<int, FunctionCache<string, ArrayType>> Array;

        [Node]
        [SmartNode]
        public readonly ValueCache<EnableCut> EnableCut;

        [Node]
        [SmartNode]
        public readonly ValueCache<IReference> ForcedReference;

        [Node]
        [SmartNode]
        public readonly ValueCache<FunctionInstanceType> FunctionInstanceType;

        [Node]
        [SmartNode]
        public readonly FunctionCache<TypeBase, ResultCache> Mutation;

        [Node]
        [SmartNode]
        public readonly FunctionCache<TypeBase, Pair> Pair;

        [Node]
        [SmartNode]
        public readonly ValueCache<PointerType> Pointer;

        public readonly ValueCache<Size> Size;

        [Node]
        [SmartNode]
        public readonly ValueCache<IEnumerable<IConversion>> SymmetricConversions;

        [Node]
        [SmartNode]
        public readonly ValueCache<TypeType> TypeType;

        [Node]
        [SmartNode]
        internal readonly FunctionCache<string, ArrayReferenceType> ArrayReferenceCache;

        public CacheContainer(TypeBase parent)
        {
            EnableCut = new(() => new(parent));
            Mutation = new(
                destination =>
                    new(category => parent.Mutation(category, destination))
            );
            ForcedReference = new(parent.GetForcedReferenceForCache);
            Pointer = new(parent.GetPointerForCache);
            Pair = new(first => new(first, parent));
            Array = new(
                count
                    =>
                    new(
                        optionsId
                            =>
                            parent.GetArrayForCache(count, optionsId)
                    )
            );

            Aligner = new(alignBits => new(parent, alignBits));
            FunctionInstanceType = new(() => new(parent));
            TypeType = new(() => new(parent));
            Size = new(parent.GetSizeForCache);
            SymmetricConversions = new(parent.GetSymmetricConversionsForCache);
            ArrayReferenceCache = new(id => new(parent, id));
        }
    }

    static int NextObjectId;

    [Node]
    [SmartNode]
    readonly CacheContainer Cache;

    [Node]
    internal Size Size => Cache.Size.Value;

    [DisableDump]
    internal EnableCut EnableCut => Cache.EnableCut.Value;

    [DisableDump]
    internal TypeBase Pointer => ForcedReference.Type();

    [DisableDump]
    internal IReference ForcedReference => Cache.ForcedReference.Value;

    [DisableDump]
    internal CodeBase ArgCode => CodeBase.Arg(this);

    [DisableDump]
    internal TypeBase AutomaticDereferenceType
        =>
            IsWeakReference
                ? CheckedReference.Converter.ResultType().AutomaticDereferenceType
                : this;

    [DisableDump]
    internal TypeBase SmartPointer => IsHollow? this : Pointer;

    [DisableDump]
    internal TypeBase Align
    {
        get
        {
            var alignBits = Root.DefaultRefAlignParam.AlignBits;
            return Size.Align(alignBits) == Size? this : Cache.Aligner[alignBits];
        }
    }

    [DisableDump]
    internal TypeType TypeType => Cache.TypeType.Value;

    [DisableDump]
    internal TypeBase FunctionInstance => Cache.FunctionInstanceType.Value;

    [DisableDump]
    internal PointerType ForcedPointer => Cache.Pointer.Value;

    [DisableDump]
    internal IReference CheckedReference => this as IReference;

    [DisableDump]
    internal bool IsWeakReference => CheckedReference != null && CheckedReference.IsWeak;

    [DisableDump]
    internal BitType BitType => Root.BitType;

    [DisableDump]
    internal TypeBase TypeForStructureElement => DeAlign(Category.Type).Type;

    [DisableDump]
    internal TypeBase TypeForArrayElement => DeAlign(Category.Type).Type;

    [DisableDump]
    IEnumerable<SearchResult> FunctionDeclarationsForType
    {
        get
        {
            var result = FunctionDeclarationForType;
            if(result != null)
                yield return SearchResult.Create(result, this);
        }
    }

    [DisableDump]
    [NotNull]
    public IEnumerable<IConversion> SymmetricConversions => Cache.SymmetricConversions.Value;

    [DisableDump]
    internal IEnumerable<IConversion> NextConversionStepOptions
        => SymmetricClosureConversions.Union(StripConversions);

    [DisableDump]
    internal IEnumerable<IConversion> SymmetricClosureConversions
        => new SymmetricClosureService(this).Execute(SymmetricClosureService.Forward);

    internal bool HasIssues => Issues?.Any() ?? false;

    protected TypeBase()
        : base(NextObjectId++) => Cache = new(this);

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    IContextReference IContextReferenceProvider.ContextReference => ForcedReference;

    /// <summary>
    ///     Gets the icon key.
    /// </summary>
    /// <value> The icon key. </value>
    string IIconKeyProvider.IconKey => "Type";

    Root IRootProvider.Value => Root;

    IImplementation ISymbolProviderForPointer<IdentityOperation>.Feature(IdentityOperation tokenClass)
        => Feature.Extension.FunctionFeature(
            (category, right, operation) => IdentityOperationResult(category, right, operation.IsEqual), tokenClass);

    [DisableDump]
    [Node]
    internal abstract Root Root { get; }

    /// <summary>
    ///     Is this an hollow type? With no data?
    /// </summary>
    [DisableDump]
    internal virtual bool IsHollow
    {
        get
        {
            NotImplementedMethod();
            return true;
        }
    }


    [DisableDump]
    internal virtual TypeBase[] ToList => new[] { this };


    [DisableDump]
    internal virtual string DumpPrintText => NodeDump;

    [DisableDump]
    internal virtual bool IsCuttingPossible => false;

    [DisableDump]
    internal virtual bool IsAligningPossible => true;

    [DisableDump]
    internal virtual bool IsPointerPossible => true;

    [DisableDump]
    internal virtual Size SimpleItemSize => null;

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
    internal virtual IImplementation CheckedFeature => this as IImplementation;

    [DisableDump]
    internal virtual bool HasQuickSize => true;

    [DisableDump]
    internal virtual TypeBase CoreType => this;

    [DisableDump]
    internal virtual TypeBase TypeForTypeOperator
        => DePointer(Category.Type).Type.DeAlign(Category.Type).Type;

    [DisableDump]
    internal virtual TypeBase ElementTypeForReference
        => DePointer(Category.Type)
            .Type
            .DeAlign(Category.Type)
            .Type;

    [DisableDump]
    internal virtual IImplementation FunctionDeclarationForType => null;

    [DisableDump]
    internal virtual IImplementation FunctionDeclarationForPointerType => null;

    [DisableDump]
    internal virtual IEnumerable<string> DeclarationOptions
        => Root
            .DefinedNames
            .Where(IsDeclarationOption)
            .Select(item => item.Id)
            .OrderBy(item => item)
            .ToArray();

    [DisableDump]
    protected virtual IEnumerable<IGenericProviderForType> GenericList
        => this.GenericListFromType();

    [DisableDump]
    protected virtual IEnumerable<IConversion> RawSymmetricConversions
    {
        get
        {
            if(IsHollow)
                yield break;

            if(IsAligningPossible && Align.Size != Size)
                yield return Feature.Extension.Conversion(AlignResult);
            if(IsPointerPossible)
                yield return Feature.Extension.Conversion(LocalReferenceResult);
        }
    }

    [DisableDump]
    protected virtual IEnumerable<IConversion> StripConversions
    {
        get { yield break; }
    }

    [DisableDump]
    internal virtual IEnumerable<IConversion> StripConversionsFromPointer
    {
        get { yield break; }
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

    [DisableDump]
    internal virtual TypeBase Weaken => null;

    internal virtual Issue[] Issues => null;

    [NotNull]
    protected virtual Size GetSize()
    {
        NotImplementedMethod();
        return Size.Zero;
    }

    internal virtual int? SmartArrayLength(TypeBase elementType)
    {
        if(IsConvertible(elementType))
            return 1;

        NotImplementedMethod(elementType);
        return null;
    }

    internal virtual Result ConvertToStableReference(Category category)
        => ArgResult(category);

    protected virtual TypeBase ReversePair(TypeBase first) => first.Cache.Pair[this];
    internal virtual TypeBase Pair(TypeBase second) => second.ReversePair(this);

    internal virtual Result Cleanup(Category category)
        => VoidCodeAndRefs(category);

    internal virtual Result Copier(Category category) => VoidCodeAndRefs(category);

    internal virtual Result ApplyTypeOperator(Result argResult)
        => argResult.Type.Conversion(argResult.CompleteCategory, this).ReplaceArg(argResult);

    protected virtual Result DeAlign(Category category) => ArgResult(category);
    protected virtual ResultCache DePointer(Category category) => ArgResult(category);

    protected virtual IReference GetForcedReferenceForCache()
    {
        (!IsHollow).Assert();
        return CheckedReference ?? ForcedPointer;
    }

    protected virtual PointerType GetPointerForCache()
    {
        (!IsHollow).Assert();
        return new(this);
    }

    protected virtual ArrayType GetArrayForCache(int count, string optionsId) => new(this, count, optionsId);

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

    internal virtual Result InstanceResult(Category category, Func<Category, Result> getRightResult)
    {
        NotImplementedMethod(category, getRightResult(Category.All));
        return null;
    }

    /// <summary>
    ///     Override this function to provide declarations of a definable for this type.
    ///     Only declaration, that are made exactly for type <see cref="TDefinable" />
    ///     should be considered.
    ///     This implementation checks if this type is symbol provider for definable.
    ///     Don't call this except in overriden versions.
    /// </summary>
    /// <typeparam name="TDefinable"></typeparam>
    /// <param name="tokenClass"></param>
    /// <returns></returns>
    internal virtual IEnumerable<SearchResult> Declarations<TDefinable>(TDefinable tokenClass)
        where TDefinable : Definable
    {
        var provider = this as ISymbolProvider<TDefinable>;
        var feature = provider?.Feature(tokenClass);
        if(feature != null)
            yield return SearchResult.Create(feature, this);
    }

    internal virtual IEnumerable<IConversion> GetForcedConversions<TDestination>(TDestination destination)
        => this is IForcedConversionProvider<TDestination> provider
            ? provider.Result(destination)
            : new IConversion[0];

    internal virtual IEnumerable<IConversion> CutEnabledConversion(NumberType destination) { yield break; }

    protected virtual CodeBase DumpPrintCode()
    {
        NotImplementedMethod();
        return null;
    }

    [NotNull]
    Size GetSizeForCache() => IsHollow? Size.Zero : GetSize();

    static Result VoidCodeAndRefs(Category category)
        => Root.VoidType.Result(category & (Category.Code | Category.Closures));

    internal ArrayType Array(int count, string options = null)
        => Cache.Array[count][options ?? ArrayType.Options.DefaultOptionsId];

    internal ArrayReferenceType ArrayReference(string optionsId)
        => Cache.ArrayReferenceCache[optionsId];

    internal Result ArrayCopier(Category category)
    {
        Copier(category).IsEmpty.Assert();
        return VoidCodeAndRefs(category);
    }

    internal Result ArrayCleanup(Category category)
    {
        Cleanup(category).IsEmpty.Assert();
        return VoidCodeAndRefs(category);
    }

    internal Result ArgResult(Category category) => Result(category, () => ArgCode, Closures.Arg);

    Result PointerArgResult(Category category) => Pointer.ArgResult(category);

    internal Result Result(Category category, IContextReference target)
    {
        if(IsHollow)
            return Result(category);

        return Result
        (
            category,
            () => CodeBase.ReferenceCode(target)
        );
    }

    internal Result Result(Category category, Result codeAndClosures)
        => Result
        (
            category,
            () => codeAndClosures.Code,
            () => codeAndClosures.Closures
        );

    internal Result Result(Category category, Func<Category, Result> getCodeAndRefs)
    {
        var localCategory = category & (Category.Code | Category.Closures);
        var codeAndClosures = getCodeAndRefs(localCategory);
        return Result
        (
            category,
            () => codeAndClosures.Code,
            () => codeAndClosures.Closures
        );
    }

    internal Result Result(Category category, Func<CodeBase> getCode = null, Func<Closures> getClosures = null)
        => new(
            category,
            getClosures, getCode, () => this);

    internal TypeBase CommonType(TypeBase elseType)
    {
        if(elseType.IsConvertible(this))
            return this;
        if(IsConvertible(elseType))
            return elseType;

        var thenConversions = ConversionService.ClosureService.Result(this);
        var elseConversions = ConversionService.ClosureService.Result(elseType);

        var combination = thenConversions
            .Merge(elseConversions, item => item.Destination)
            .Where(item => item.Item2 != null && item.Item3 != null)
            .GroupBy(item => item.Item2.Elements.Length + item.Item3.Elements.Length)
            .OrderBy(item => item.Key)
            .First()
            .ToArray();

        if(combination.Length == 1)
            return combination.Single().Item1;

        NotImplementedMethod
        (
            elseType,
            nameof(combination),
            combination
        );
        return null;
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
        if(IsHollow)
            return ArgResult(category);

        return ForcedPointer
            .Result
            (
                category,
                LocalReferenceCode,
                Closures.Arg
            );
    }

    CodeBase LocalReferenceCode()
        => ArgCode
            .LocalReference(this);

    internal Result ContextAccessResult(Category category, IContextReference target, Func<Size> getOffset)
    {
        if(IsHollow)
            return Result(category);

        return Result
        (
            category,
            () => CodeBase.ReferenceCode(target).ReferencePlus(getOffset()).DePointer(Size)
        );
    }

    internal Result GenericDumpPrintResult(Category category)
    {
        var searchResults = SmartPointer
            .Declarations<DumpPrintToken>(null)
            .SingleOrDefault();

        if(searchResults == null)
        {
            NotImplementedMethod(category);
            return null;
        }

        return searchResults.SpecialExecute(category);
    }

    internal Result CreateArray(Category category, string optionsId = null) => Align
        .Array(1, optionsId)
        .Pointer
        .Result(category, PointerArgResult(category));

    internal bool IsConvertible(TypeBase destination)
        => ConversionService.FindPath(this, destination) != null;

    internal Result Conversion(Category category, TypeBase destination)
    {
        if(Category.Type.Replenished().Contains(category))
            return destination.SmartPointer.Result(category);

        var path = ConversionService.FindPath(this, destination);
        return path == null
            ? ArgResult(category).InvalidConversion(destination)
            : path.Execute(category | Category.Type);
    }

    Result Mutation(Category category, TypeBase destination)
        => destination.Result(category, ArgResult);

    internal ResultCache Mutation(TypeBase destination)
        => Cache.Mutation[destination];

    internal Result DumpPrintTypeNameResult(Category category) => Root.VoidType
        .Result
        (
            category,
            () => CodeBase.DumpPrintText(DumpPrintText),
            Closures.Void
        );

    internal TypeBase SmartUn<T>()
        where T : IConversion
        => this is T? ((IConversion)this).Result(Category.Type).Type : this;

    internal Result ResultFromPointer(Category category, TypeBase resultType) => resultType
        .Pointer
        .Result(category, ObjectResult);

    /// <summary>
    ///     Call this function to get declarations of definable for this type.
    /// </summary>
    /// <param name="definable"></param>
    /// <returns></returns>
    internal IEnumerable<SearchResult> DeclarationsForType(Definable definable)
    {
        if(definable == null)
            return FunctionDeclarationsForType;

        return definable
            .MakeGeneric
            .SelectMany(g => g.Declarations(this))
            .ToArray();
    }

    /// <summary>
    ///     Call this function to get declarations of definable for this type
    ///     and its close relatives (see <see cref="ConversionService.CloseRelativeConversions" />).
    /// </summary>
    /// <param name="tokenClass"></param>
    /// <returns></returns>
    IEnumerable<SearchResult> DeclarationsForTypeAndCloseRelatives(Definable tokenClass)
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

    bool IsDeclarationOption(Definable tokenClass)
        => DeclarationsForType(tokenClass).Any();

    Result AlignResult(Category category) => Align.Result(category, () => ArgCode.Align(), Closures.Arg);

    IEnumerable<IConversion> GetSymmetricConversionsForCache()
        => RawSymmetricConversions
            .ToDictionary(x => x.ResultType())
            .Values;

    internal IEnumerable<IConversion> GetForcedConversions(TypeBase destination)
    {
        var genericProviderForTypes = destination
            .GenericList
            .ToArray();
        var result = genericProviderForTypes
            .SelectMany(g => g.GetForcedConversions(this).ToArray())
            .ToArray();

        result.All(f => f.Source == this).Assert();
        result.All(f => f.ResultType() == destination).Assert();
        (result.Length <= 1).Assert();

        return result;
    }

    protected Result DumpPrintTokenResult(Category category)
        => Root.VoidType.Result(category, DumpPrintCode)
            .ReplaceArg(DereferencesObjectResult(category));

    Result DereferencesObjectResult(Category category)
        =>
            IsHollow
                ? Result(category)
                : Pointer.Result(category | Category.Type, ForcedReference).DereferenceResult;

    internal Result ObjectResult(Category category)
        => IsHollow? Result(category) : Pointer.Result(category | Category.Type, ForcedReference);

    Result IssueResult
        (Category category, IssueId issueId, SourcePart token, SearchResult[] declarations, Issue[] leftIssues)
        => issueId
            .GetResult(category, token, this, declarations, leftIssues);

    internal Result Execute
    (
        Category category,
        ResultCache left,
        SourcePart currentTarget,
        Definable definable,
        ContextBase context,
        ValueSyntax right
    )
        => ExecuteDeclaration
        (
            definable,
            result => result.Execute(category, left, currentTarget, context, right),
            (issueId, declarations) => IssueResult(category, issueId, currentTarget, declarations, left.Issues)
        );

    TResult ExecuteDeclaration<TResult>
    (
        Definable definable,
        Func<SearchResult, TResult> execute,
        Func<IssueId, SearchResult[], TResult> onError
    )
    {
        var searchResults
            = DeclarationsForTypeAndCloseRelatives(definable)
                .RemoveLowPriorityResults()
                .ToArray();

        switch(searchResults.Length)
        {
            case 0:
                return onError(IssueId.MissingDeclarationForType, null);
            case 1:
                return execute(searchResults.First());
            default:
                return onError(IssueId.AmbiguousSymbol, searchResults);
        }
    }

    Result IdentityOperationResult(Category category, TypeBase right, bool isEqual)
    {
        if(AutomaticDereferenceType == right.AutomaticDereferenceType)
            return IdentityOperationResult(category, isEqual)
                .ReplaceArg(c => right.Conversion(c, AutomaticDereferenceType.Pointer));

        return Root.BitType.Result
        (
            category,
            () => CodeBase.BitsConst(BitsConst.Convert(isEqual))
        );
    }

    Result IdentityOperationResult(Category category, bool isEqual)
    {
        var result = Root.BitType.Result
        (
            category,
            () => IdentityOperationCode(isEqual),
            Closures.Arg
        );

        var leftResult = ObjectResult(category | Category.Type).Conversion(Align);
        var rightResult = ObjectResult(category | Category.Type).Conversion(Align);
        var pair = leftResult + rightResult;
        return result.ReplaceArg(pair);
    }

    CodeBase IdentityOperationCode(bool isEqual) => Align
        .Pair(Align)
        .ArgCode
        .Add(new IdentityTestCode(isEqual, Size.Bit, Align.Size));
}

// ReSharper disable CommentTypo
// Krautpuster
// Gurkennudler