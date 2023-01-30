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
                    new(category => parent.GetMutation(category, destination))
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
    internal CodeBase ArgumentCode => CodeBase.GetArgument(this);

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
            return Size.GetAlign(alignBits) == Size? this : Cache.Aligner[alignBits];
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
    internal TypeBase TypeForStructureElement => GetDeAlign(Category.Type).Type;

    [DisableDump]
    internal TypeBase TypeForArrayElement => GetDeAlign(Category.Type).Type;

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

    IImplementation ISymbolProviderForPointer<IdentityOperation>.GetFeature(IdentityOperation tokenClass)
        => Feature.Extension.FunctionFeature(
            (category, right, operation) => GetIdentityOperationResult(category, right, operation.IsEqual), tokenClass);

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
        => GetDePointer(Category.Type).Type.GetDeAlign(Category.Type).Type;

    [DisableDump]
    internal virtual TypeBase ElementTypeForReference
        => GetDePointer(Category.Type)
            .Type
            .GetDeAlign(Category.Type)
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
                yield return Feature.Extension.Conversion(GetAlignedResult);
            if(IsPointerPossible)
                yield return Feature.Extension.Conversion(GetLocalReferenceResult);
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

    internal virtual int? GetSmartArrayLength(TypeBase elementType)
    {
        if(IsConvertible(elementType))
            return 1;

        NotImplementedMethod(elementType);
        return null;
    }

    internal virtual Result GetConversionToStableReference(Category category)
        => GetArgumentResult(category);

    protected virtual TypeBase GetReversePair(TypeBase first) => first.Cache.Pair[this];
    internal virtual TypeBase GetPair(TypeBase second) => second.GetReversePair(this);

    internal virtual Result GetCleanup(Category category)
        => GetVoidCodeAndRefs(category);

    internal virtual Result GetCopier(Category category) => GetVoidCodeAndRefs(category);

    internal virtual Result GetTypeOperatorApply(Result argResult)
        => argResult.Type.GetConversion(argResult.CompleteCategory, this).ReplaceArgument(argResult);

    protected virtual Result GetDeAlign(Category category) => GetArgumentResult(category);
    protected virtual ResultCache GetDePointer(Category category) => GetArgumentResult(category);

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

    internal virtual Result GetConstructorResult(Category category, TypeBase argumentsType)
    {
        StartMethodDump(false, category, argumentsType);
        try
        {
            BreakExecution();
            var result = argumentsType.GetConversion(category, this);
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal virtual Result GetInstanceResult(Category category, Func<Category, Result> getRightResult)
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
    internal virtual IEnumerable<SearchResult> GetDeclarations<TDefinable>(TDefinable tokenClass)
        where TDefinable : Definable
    {
        var provider = this as ISymbolProvider<TDefinable>;
        var feature = provider?.GetFeature(tokenClass);
        if(feature != null)
            yield return SearchResult.Create(feature, this);
    }

    internal virtual IEnumerable<IConversion> GetForcedConversions<TDestination>(TDestination destination)
        => this is IForcedConversionProvider<TDestination> provider
            ? provider.GetResult(destination)
            : new IConversion[0];

    internal virtual IEnumerable<IConversion> GetCutEnabledConversion(NumberType destination) { yield break; }

    [DisableDump]
    protected virtual CodeBase DumpPrintCode
    {
        get
        {
            NotImplementedMethod();
            return null;
        }
    }

    [NotNull]
    Size GetSizeForCache() => IsHollow? Size.Zero : GetSize();

    static Result GetVoidCodeAndRefs(Category category)
        => Root.VoidType.GetResult(category & (Category.Code | Category.Closures));

    internal ArrayType GetArray(int count, string options = null)
        => Cache.Array[count][options ?? ArrayType.Options.DefaultOptionsId];

    internal ArrayReferenceType GetArrayReference(string optionsId)
        => Cache.ArrayReferenceCache[optionsId];

    internal Result GetArrayCopier(Category category)
    {
        GetCopier(category).IsEmpty.Assert();
        return GetVoidCodeAndRefs(category);
    }

    internal Result GetArrayCleanup(Category category)
    {
        GetCleanup(category).IsEmpty.Assert();
        return GetVoidCodeAndRefs(category);
    }

    internal Result GetArgumentResult(Category category) => GetResult(category, () => ArgumentCode, Closures.Argument);

    Result GetPointerArgumentResult(Category category) => Pointer.GetArgumentResult(category);

    internal Result GetResult(Category category, IContextReference target)
    {
        if(IsHollow)
            return GetResult(category);

        return GetResult
        (
            category,
            () => CodeBase.GetReferenceCode(target)
        );
    }

    internal Result GetResult(Category category, Result codeAndClosures)
        => GetResult
        (
            category,
            () => codeAndClosures.Code,
            () => codeAndClosures.Closures
        );

    internal Result GetResult(Category category, Func<Category, Result> getCodeAndRefs)
    {
        var localCategory = category & (Category.Code | Category.Closures);
        var codeAndClosures = getCodeAndRefs(localCategory);
        return GetResult
        (
            category,
            () => codeAndClosures.Code,
            () => codeAndClosures.Closures
        );
    }

    internal Result GetResult(Category category, Func<CodeBase> getCode = null, Func<Closures> getClosures = null)
        => new(
            category,
            getClosures, getCode, () => this);

    internal TypeBase GetCommonType(TypeBase elseType)
    {
        if(elseType.IsConvertible(this))
            return this;
        if(IsConvertible(elseType))
            return elseType;

        var thenConversions = ConversionService.ClosureService.GetResult(this);
        var elseConversions = ConversionService.ClosureService.GetResult(elseType);

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

    internal int GetArrayLength(TypeBase elementType)
    {
        var length = GetSmartArrayLength(elementType);
        if(length != null)
            return length.Value;

        NotImplementedMethod(elementType);
        return -1;
    }

    internal Result GetLocalReferenceResult(Category category)
    {
        if(IsHollow)
            return GetArgumentResult(category);

        return ForcedPointer
            .GetResult
            (
                category,
                GetLocalReferenceCode,
                Closures.Argument
            );
    }

    CodeBase GetLocalReferenceCode()
        => ArgumentCode
            .GetLocalReference(this);

    internal Result GetContextAccessResult(Category category, IContextReference target, Func<Size> getOffset)
    {
        if(IsHollow)
            return GetResult(category);

        return GetResult
        (
            category,
            () => CodeBase.GetReferenceCode(target).GetReferenceWithOffset(getOffset()).GetDePointer(Size)
        );
    }

    internal Result GetGenericDumpPrintResult(Category category)
    {
        var searchResults = SmartPointer
            .GetDeclarations<DumpPrintToken>(null)
            .SingleOrDefault();

        if(searchResults == null)
        {
            NotImplementedMethod(category);
            return null;
        }

        return searchResults.SpecialExecute(category);
    }

    internal Result CreateArray(Category category, string optionsId = null) => Align
        .GetArray(1, optionsId)
        .Pointer
        .GetResult(category, GetPointerArgumentResult(category));

    internal bool IsConvertible(TypeBase destination)
        => ConversionService.FindPath(this, destination) != null;

    internal Result GetConversion(Category category, TypeBase destination)
    {
        if(Category.Type.Replenished().Contains(category))
            return destination.SmartPointer.GetResult(category);

        var path = ConversionService.FindPath(this, destination);
        return path == null
            ? GetArgumentResult(category).GetInvalidConversion(destination)
            : path.Execute(category | Category.Type);
    }

    Result GetMutation(Category category, TypeBase destination)
        => destination.GetResult(category, GetArgumentResult);

    internal ResultCache GetMutation(TypeBase destination)
        => Cache.Mutation[destination];

    internal Result GetDumpPrintTypeNameResult(Category category) => Root.VoidType
        .GetResult
        (
            category,
            () => CodeBase.GetDumpPrintText(DumpPrintText),
            Closures.Void
        );

    internal TypeBase GetSmartUn<T>()
        where T : IConversion
        => this is T? ((IConversion)this).GetResult(Category.Type).Type : this;

    internal Result GetResultFromPointer(Category category, TypeBase resultType) => resultType
        .Pointer
        .GetResult(category, GetObjectResult);

    /// <summary>
    ///     Call this function to get declarations of definable for this type.
    /// </summary>
    /// <param name="definable"></param>
    /// <returns></returns>
    internal IEnumerable<SearchResult> GetDeclarationsForType(Definable definable)
    {
        if(definable == null)
            return FunctionDeclarationsForType;

        return definable
            .MakeGeneric
            .SelectMany(g => g.GetDeclarations(this))
            .ToArray();
    }

    /// <summary>
    ///     Call this function to get declarations of definable for this type
    ///     and its close relatives (see <see cref="ConversionService.CloseRelativeConversions" />).
    /// </summary>
    /// <param name="tokenClass"></param>
    /// <returns></returns>
    IEnumerable<SearchResult> GetDeclarationsForTypeAndCloseRelatives(Definable tokenClass)
    {
        var result = GetDeclarationsForType(tokenClass).ToArray();
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
        => GetDeclarationsForType(tokenClass).Any();

    Result GetAlignedResult
        (Category category) => Align.GetResult(category, () => ArgumentCode.GetAlign(), Closures.Argument);

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

    protected Result GetDumpPrintTokenResult(Category category)
        => Root.VoidType.GetResult(category, () => DumpPrintCode)
            .ReplaceArgument(GetDereferencesObjectResult(category));

    Result GetDereferencesObjectResult(Category category)
        =>
            IsHollow
                ? GetResult(category)
                : Pointer.GetResult(category | Category.Type, ForcedReference).DereferenceResult;

    internal Result GetObjectResult(Category category)
        => IsHollow? GetResult(category) : Pointer.GetResult(category | Category.Type, ForcedReference);

    Result GetIssueResult
        (Category category, IssueId issueId, SourcePart token, SearchResult[] declarations, Issue[] leftIssues)
        => issueId
            .GetResult(category, token, this, declarations, leftIssues);

    internal(Result, IImplementation) GetResult
    (
        Category category,
        ResultCache left,
        SourcePart currentTarget,
        Definable definable,
        ContextBase context,
        ValueSyntax right
    )
        => FindDeclarationAndExecute
        (
            definable,
            result => result.Execute(category, left, currentTarget, context, right),
            (issueId, declarations) => GetIssueResult(category, issueId, currentTarget, declarations, left.Issues)
        );

    (Result, IImplementation) FindDeclarationAndExecute
    (
        Definable definable,
        Func<SearchResult, Result> execute,
        Func<IssueId, SearchResult[], Result> onError
    )
    {
        var searchResults
            = GetDeclarationsForTypeAndCloseRelatives(definable)
                .RemoveLowPriorityResults()
                .ToArray();

        switch(searchResults.Length)
        {
            case 0:
                return (onError(IssueId.MissingDeclarationForType, null), null);
            case 1:
                return (execute(searchResults.First()), searchResults.First());
            default:
                return (onError(IssueId.AmbiguousSymbol, searchResults), null);
        }
    }

    Result GetIdentityOperationResult(Category category, TypeBase right, bool isEqual)
    {
        if(AutomaticDereferenceType == right.AutomaticDereferenceType)
            return GetIdentityOperationResult(category, isEqual)
                .ReplaceArgument(c => right.GetConversion(c, AutomaticDereferenceType.Pointer));

        return Root.BitType.GetResult
        (
            category,
            () => CodeBase.GetBitsConst(BitsConst.Convert(isEqual))
        );
    }

    Result GetIdentityOperationResult(Category category, bool isEqual)
    {
        var result = Root.BitType.GetResult
        (
            category,
            () => GetIdentityOperationCode(isEqual),
            Closures.Argument
        );

        var leftResult = GetObjectResult(category | Category.Type).GetConversion(Align);
        var rightResult = GetObjectResult(category | Category.Type).GetConversion(Align);
        var pair = leftResult + rightResult;
        return result.ReplaceArgument(pair);
    }

    CodeBase GetIdentityOperationCode(bool isEqual) => Align
        .GetPair(Align)
        .ArgumentCode
        .Concat(new IdentityTestCode(isEqual, Size.Bit, Align.Size));
}

// ReSharper disable CommentTypo
// Krautpuster
// Gurkennudler