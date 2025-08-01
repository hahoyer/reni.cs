using System.Diagnostics;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Helper;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Type;

abstract partial class TypeBase
    : DumpableObject
        , IContextReferenceProvider
        , IIconKeyProvider
        , ISearchTarget
        , ValueCache.IContainer
        , IMultiSymbolProviderForPointer<IdentityOperation>
        , ISymbolProviderForPointer<ForeignCode>

{
    static int NextObjectId;

    [Node]
    [SmartNode]
    internal readonly LinkedTypes Make;

    [Node]
    [SmartNode]
    internal readonly ConversionSetup Conversion;

    [Node]
    [SmartNode]
    internal readonly SetupOverView OverView;

    [Node]
    [SmartNode]
    readonly CacheContainer Cache;

    protected TypeBase()
        : base(NextObjectId++)
    {
        Cache = new(this);
        Make = new(this);
        OverView = new(this);
        Conversion = new(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    ValueCache ValueCache.IContainer.Cache { get; } = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IContextReference IContextReferenceProvider.ContextReference => Make.ForcedReference;

    /// <summary>
    ///     Gets the icon key.
    /// </summary>
    /// <value> The icon key. </value>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string IIconKeyProvider.IconKey => "Type";

    IImplementation IMultiSymbolProviderForPointer<IdentityOperation>.GetFeature(IdentityOperation tokenClass)
        => Feature.Extension.FunctionFeature(
            (category, right, operation) => GetIdentityOperationResult(category, right, operation.IsEqual), tokenClass);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IImplementation ISymbolProviderForPointer<ForeignCode>.Feature
        => this.CachedValue(() => new ForeignCodeFeature(Root));

    [DisableDump]
    [Node]
    internal abstract Root Root { get; }

    protected virtual TypeBase GetTypeType() => Cache.TypeType.Value;

    /// <summary>
    ///     Is this a hollow type? With no data?
    /// </summary>
    protected virtual bool GetIsHollow()
    {
        NotImplementedMethod();
        return true;
    }


    internal virtual IEnumerable<TypeBase> GetToList() => [this];

    internal string NameDump => GetNameDump();
    
    protected virtual string GetNameDump() => GetNodeDump();

    protected virtual string GetDumpPrintText() => GetNodeDump();

    protected virtual bool GetIsAligningPossible() => true;

    protected virtual bool GetIsPointerPossible() => true;

    internal virtual Size? GetTextItemSize() => null;

    internal virtual CompoundView FindRecentCompoundView()
    {
        NotImplementedMethod();
        return null!;
    }

    internal virtual IImplementation? GetCheckedFeature() => this as IImplementation;

    protected virtual bool GetHasQuickSize() => true;

    protected virtual TypeBase GetTagTargetType() => this;

    internal virtual TypeBase GetTypeForTypeOperator()
        => GetDePointer(Category.Type).Type
            .GetDeAlign(Category.Type).Type;

    internal virtual IImplementation? GetFunctionDeclarationForType() => null;

    internal virtual IImplementation? GetFunctionDeclarationForPointerType() => null;

    [DisableDump]
    internal virtual IEnumerable<string> DeclarationOptions
        => Root
            .DefinedNames
            .Where(IsDeclarationOption)
            .Select(item => item.Id)
            .OrderBy(item => item)
            .ToArray();

    protected virtual IEnumerable<IGenericProviderForType> GetGenericProviders()
        => Feature.Extension.GetGenericProviders(this);

    protected virtual IEnumerable<IConversion> GetSymmetricConversions()
    {
        if(GetIsHollow())
            yield break;

        if(GetIsAligningPossible() && Make.Align.OverView.Size != OverView.Size)
            yield return Feature.Extension.Conversion(GetAlignedResult);
        if(GetIsPointerPossible())
            yield return Feature.Extension.Conversion(GetLocalReferenceResult);
    }

    protected virtual IEnumerable<IConversion> GetStripConversions() { yield break; }
    protected virtual IEnumerable<IConversion> GetStripConversionsFromPointer() { yield break; }

    [DisableDump]
    internal virtual ContextBase ToContext
    {
        get
        {
            NotImplementedMethod();
            return null!;
        }
    }

    internal virtual TypeBase? UnReferenceStableReference() => null;

    internal virtual Issue[] Issues => [];

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

    protected virtual TypeBase GetReversePair(TypeBase first) => first.Cache.Pair[this];
    internal virtual TypeBase GetPair(TypeBase second) => second.GetReversePair(this);

    internal virtual Result GetCleanup(Category category)
        => GetVoidCodeAndRefs(category);

    internal virtual Result GetCopier(Category category) => GetVoidCodeAndRefs(category);

    internal virtual Result GetTypeOperatorApply(Result argResult)
        => argResult
            .Type
            .ExpectNotNull(()
                => (null, "GetTypeOperatorApply requires type category for argument:\n " + argResult.Dump()))
            .GetConversion(argResult.CompleteCategory, this)
            .ReplaceArguments(argResult);

    protected virtual Result GetDeAlign(Category category) => GetArgumentResult(category);
    protected virtual ResultCache GetDePointer(Category category) => GetArgumentResult(category);

    protected virtual IReference GetForcedReferenceForCache()
    {
        (!GetIsHollow()).Assert();
        return Make.CheckedReference ?? Make.ForcedPointer;
    }

    protected virtual PointerType GetPointerForCache()
    {
        (!GetIsHollow()).Assert();
        return new(this);
    }

    protected virtual ArrayType GetArrayForCache(int count, string optionsId)
        => new(this, count, optionsId);

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
        return null!;
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
    internal virtual IEnumerable<SearchResult> GetDeclarations<TDefinable>(TDefinable? tokenClass = null)
        where TDefinable : Definable
    {
        if((this as ISymbolProvider<TDefinable>)?.Feature is { } feature)
            yield return SearchResult.Create(feature, this);

        if(tokenClass == null)
            yield break;

        if((this as IMultiSymbolProvider<TDefinable>)?.GetFeature(tokenClass) is { } multiFeature)
            yield return SearchResult.Create(multiFeature, this);
    }

    internal virtual IEnumerable<IConversion> GetForcedConversions<TDestination>
        (TDestination destination) => this is IForcedConversionProvider<TDestination> provider
        ? provider.GetResult(destination)
        : new IConversion[0];

    internal virtual IEnumerable<IConversion> GetCutEnabledConversion(NumberType destination) { yield break; }

    [DisableDump]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    protected virtual CodeBase DumpPrintCode
    {
        get
        {
            NotImplementedMethod();
            return null!;
        }
    }

    [DisableDump]
    protected virtual IssueId MissingDeclarationIssueId
        => IssueId.MissingDeclarationForType;

    internal virtual object GetDataValue(BitsConst data)
    {
        NotImplementedMethod(data);
        return default!;
    }

    Issue GetMissingDeclarationIssue(SourcePart position)
        => Cache.MissingDeclarationIssue[position];

    Issue GetMissingDeclarationIssueForCache(SourcePart position)
        => MissingDeclarationIssueId.GetIssue(Root, position, this);

    Size GetSizeForCache()
    {
        StopByObjectIds();
        return GetIsHollow()? Size.Zero : GetSize();
    }

    Result GetVoidCodeAndRefs(Category category)
        => Root.VoidType.GetResult(category & (Category.Code | Category.Closures));

    internal ArrayType GetArray(int count, string? options = null)
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

    internal Result GetArgumentResult(Category category)
        => GetResult(category, () => Make.ArgumentCode, Closures.GetArgument);

    Result GetPointerArgumentResult(Category category) => Make.Pointer.GetArgumentResult(category);

    internal Result GetResult(Category category, IContextReference target)
    {
        if(GetIsHollow())
            return GetResult(category);

        return GetResult
        (
            category,
            target.GetCode
        );
    }

    internal Result GetResult(Category category, Result codeAndClosures)
        => GetResult
        (
            category,
            () => codeAndClosures.Code.ExpectNotNull(),
            () => codeAndClosures.Closures.ExpectNotNull()
        );

    internal Result GetResult(Category category, Func<Category, Result> getCodeAndRefs)
    {
        var localCategory = category & (Category.Code | Category.Closures);
        var codeAndClosures = getCodeAndRefs(localCategory);
        return GetResult
        (
            category,
            () => codeAndClosures.Code.ExpectNotNull(),
            () => codeAndClosures.Closures.ExpectNotNull()
        );
    }

    internal Result GetResult(Category category, Func<CodeBase>? getCode = null, Func<Closures>? getClosures = null)
        => new(category, getClosures, getCode, () => this);

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
            .GroupBy(item => item.Item2!.Elements.Length + item.Item3!.Elements.Length)
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
        return null!;
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
        if(GetIsHollow())
            return GetArgumentResult(category);

        return Make.ForcedPointer
            .GetResult
            (
                category,
                GetLocalReferenceCode,
                Closures.GetArgument
            );
    }

    CodeBase GetLocalReferenceCode()
        => Make.ArgumentCode
            .GetLocalReference(this);

    internal Result GetContextAccessResult(Category category, IContextReference target, Func<Size> getOffset)
    {
        if(GetIsHollow())
            return GetResult(category);

        return GetResult
        (
            category,
            () => target.GetCode().GetReferenceWithOffset(getOffset()).GetDePointer(OverView.Size)
        );
    }

    internal Result GetGenericDumpPrintResult(Category category)
    {
        var searchResults = (GetIsHollow()? this : Make.Pointer)
            .GetDeclarations<DumpPrintToken>()
            .SingleOrDefault();

        if(searchResults == null)
        {
            NotImplementedMethod(category);
            return null!;
        }

        return searchResults.SpecialExecute(category);
    }

    internal bool IsConvertible(TypeBase destination) // todo: rename to IsConvertibleTo
        => ConversionService.FindPath(this, destination) != null;

    internal Result GetConversion(Category category, TypeBase destination) // todo: rename to GetConversionTo
    {
        if(Category.Type.Replenished().Contains(category))
            return (destination.GetIsHollow()? destination : destination.Make.Pointer).GetResult(category);

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
            () => GetDumpPrintText().GetDumpPrintTextCode(),
            Closures.GetVoid
        );

    internal TypeBase GetSmartUn<T>()
        where T : IConversion
        => this is T? ((IConversion)this).GetResult(Category.Type).Type : this;

    internal Result GetResultFromPointer(Category category, TypeBase resultType)
        => resultType.Make.Pointer
            .GetResult(category, GetObjectResult);

    /// <summary>
    ///     Call this function to get declarations of definable for this type.
    /// </summary>
    /// <param name="definable"></param>
    /// <returns></returns>
    internal IEnumerable<SearchResult> GetDeclarationsForType(Definable? definable)
    {
        if(definable != null)
            return definable
                .MakeGeneric
                .SelectMany(g => g.GetDeclarations(this))
                .ToArray();

        var result = GetFunctionDeclarationForType();
        if(result == null)
            return [];
        return [SearchResult.Create(result, this)];
    }

    /// <summary>
    ///     Call this function to get declarations of definable for this type
    ///     and its close relatives (see <see cref="ConversionService.CloseRelativeConversions" />).
    /// </summary>
    /// <param name="tokenClass"></param>
    /// <returns></returns>
    IEnumerable<SearchResult> GetDeclarationsForTypeAndCloseRelatives(Definable? tokenClass)
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

    bool IsDeclarationOption(Definable? tokenClass)
        => GetDeclarationsForType(tokenClass).Any();

    Result GetAlignedResult
        (Category category) => Make.Align.GetResult(category, () => Make.ArgumentCode.GetAlign(), Closures.GetArgument);

    IConversion[] GetSymmetricConversionsForCache()
        => GetSymmetricConversions()
            .ToDictionary(x => x.ResultType())
            .Values
            .ToArray();

    internal IEnumerable<IConversion> GetForcedConversions(TypeBase destination)
    {
        var genericProviderForTypes = destination.GetGenericProviders()
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
            .ReplaceArguments(GetDereferencesObjectResult(category));

    Result GetDereferencesObjectResult(Category category)
        =>
            GetIsHollow()
                ? GetResult(category)
                : Make.Pointer.GetResult(category | Category.Type, Make.ForcedReference).Dereference;

    internal Result GetObjectResult(Category category)
        => GetIsHollow()
            ? GetResult(category) 
            : Make.Pointer.GetResult(category | Category.Type, Make.ForcedReference);

    internal Result GetResult
    (
        Category category,
        ResultCache left,
        SourcePart currentTarget,
        Definable? definable,
        ContextBase context,
        ValueSyntax? right
    )
    {
        var searchResults
            = GetDeclarationsForTypeAndCloseRelatives(definable)
                .RemoveLowPriorityResults()
                .ToArray();

        var count = searchResults.Length;
        if(count == 1)
            return searchResults
                .First()
                .Execute(category, left, currentTarget, context, right);

        var issue = count == 0
            ? GetMissingDeclarationIssue(currentTarget)
            : IssueId.AmbiguousSymbol.GetIssue(Root, currentTarget, this, searchResults);

        return new(category, [..left.Issues, issue]);
    }

    Result GetIdentityOperationResult(Category category, TypeBase right, bool isEqual)
    {
        if(Make.AutomaticDereferenceType == right.Make.AutomaticDereferenceType)
            return GetIdentityOperationResult(category, isEqual)
                .ReplaceArguments(c => right.GetConversion(c, Make.AutomaticDereferenceType.Make.Pointer));

        return Root.BitType.GetResult
        (
            category,
            () => Code.Extension.GetCode(BitsConst.Convert(isEqual))
        );
    }

    Result GetIdentityOperationResult(Category category, bool isEqual)
    {
        var result = Root.BitType.GetResult
        (
            category,
            () => GetIdentityOperationCode(isEqual),
            Closures.GetArgument
        );

        var leftResult = GetObjectResult(category | Category.Type).GetConversion(Make.Align);
        var rightResult = GetObjectResult(category | Category.Type).GetConversion(Make.Align);
        return result.ReplaceArguments((leftResult + rightResult)!);
    }

    CodeBase GetIdentityOperationCode(bool isEqual) => Make.Align
        .GetPair(Make.Align)
        .Make.ArgumentCode
        .Concat(new IdentityTestCode(isEqual, Size.Bit, Make.Align.OverView.Size));

    internal Result GetConversionToText()
    {
        StartMethodDump(false);
        try
        {
            var textItemType = Root.BitType.GetArray(Size.TextItemSize.ToInt()).TextItem;
            var count = GetArrayLength(textItemType);
            var textType = textItemType.GetArray(count).TextItem;
            var result = GetConversion(Category.All, textType);
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal Result GetConversionToNumber(int count)
    {
        var targetType = (Root.BitType * count).Number;
        var result = GetConversion(Category.All, targetType);
        return result;
    }

    public static ArrayType operator *(TypeBase target, int count) => target.GetArray(count);
}

// ReSharper disable CommentTypo
// Krautpuster
// Gurkennudler
