using System.Diagnostics;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Feature;
using Reni.Struct;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.Context;

/// <summary>
///     Base class for compiler environments
/// </summary>
abstract class ContextBase
    : DumpableObject, ResultCache.IResultProvider, IIconKeyProvider, ValueCache.IContainer, IRootProvider
{
    internal sealed class ResultProvider
        : DumpableObject
            , ResultCache.IResultProvider
    {
        static int NextObjectId;

        internal readonly ContextBase Context;
        internal readonly ValueSyntax Syntax;

        [EnableDumpExcept(false)]
        readonly bool AsReference;

        [EnableDump]
        string ContextId => Context.NodeDump;

        [EnableDump]
        int SyntaxObjectId => Syntax.ObjectId;

        internal ResultProvider(ContextBase context, ValueSyntax syntax, bool asReference = false)
            : base(NextObjectId++)
        {
            Context = context;
            Syntax = syntax;
            AsReference = asReference;
            StopByObjectIds();
        }

        Result ResultCache.IResultProvider.Execute(Category category)
            => AsReference
                ? Context.GetResultAsReference(category, Syntax)
                : Context.GetResultForCache(category, Syntax);
    }

    internal sealed class Cache : DumpableObject, IIconKeyProvider
    {
        [Node]
        [SmartNode]
        internal readonly ResultCache AsObject;

        [Node]
        [SmartNode]
        internal readonly FunctionCache<CompoundSyntax, Compound> Compounds;

        [Node]
        [DisableDump]
        internal readonly ValueCache<IFunctionContext> RecentFunctionContextObject;

        [Node]
        [DisableDump]
        internal readonly ValueCache<CompoundView> RecentStructure;

        [Node]
        [SmartNode]
        internal readonly FunctionCache<ValueSyntax, ResultCache> ResultAsReferenceCache;

        [Node]
        [SmartNode]
        internal readonly FunctionCache<ValueSyntax, ResultCache> ResultCache;

        public Cache(ContextBase target)
        {
            ResultCache = new(target.GetResultCacheForCache);
            ResultAsReferenceCache = new(target.GetResultAsReferenceCacheForCache);
            RecentStructure = new(target.GetRecentCompoundView);
            RecentFunctionContextObject = new(target.GetRecentFunctionContext);
            Compounds = new(container => new(container, target));

            AsObject = new(target);
        }

        [DisableDump]
        string IIconKeyProvider.IconKey => "Cache";
    }

    static int NextId;

    [DisableDump]
    [Node]
    internal readonly Cache CacheObject;

    [DisableDump]
    internal CompoundView FindRecentCompoundView => CacheObject.RecentStructure.Value;

    [DisableDump]
    internal IFunctionContext FindRecentFunctionContextObject
        => CacheObject.RecentFunctionContextObject.Value;

    [DisableDump]
    public string Format => ParentChain.Select(item => item.LevelFormat).Stringify(" in ");

    protected ContextBase()
        : base(NextId++) => CacheObject = new(this);

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    string IIconKeyProvider.IconKey => "Context";


    Result ResultCache.IResultProvider.Execute(Category category)
        => FindRecentCompoundView.ObjectPointerViaContext(category);

    Root IRootProvider.Value => RootContext;

    [DisableDump]
    [Node]
    internal abstract Root RootContext { get; }

    [DisableDump]
    internal abstract bool IsRecursionMode { get; }

    [DisableDump]
    protected abstract string LevelFormat { get; }

    internal abstract string ContextIdentificationDump { get; }

    [DisableDump]
    internal virtual IEnumerable<string> DeclarationOptions
    {
        get
        {
            NotImplementedMethod();
            return null;
        }
    }

    internal virtual IEnumerable<ContextBase> ParentChain
    {
        get { yield return this; }
    }

    internal virtual CompoundView GetRecentCompoundView()
    {
        NotImplementedMethod();
        return null;
    }

    internal virtual IFunctionContext GetRecentFunctionContext()
    {
        NotImplementedMethod();
        return null;
    }

    internal virtual IEnumerable<IImplementation> GetDeclarations<TDefinable>
        (TDefinable tokenClass)
        where TDefinable : Definable
    {
        var provider = this as ISymbolProviderForPointer<TDefinable>;
        var feature = provider?.GetFeature(tokenClass);
        if(feature != null)
            yield return feature;
    }

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + ContextIdentificationDump + ")";


    [UsedImplicitly]
    internal int GetPacketCount(Size size)
        => size.GetPacketCount(Root.DefaultRefAlignParam.AlignBits);

    internal ContextBase GetCompoundPositionContext(CompoundSyntax container, int? position = null)
        => GetCompoundView(container, position)?.CompoundContext;

    internal CompoundView GetCompoundView(CompoundSyntax syntax, int? accessPosition = null)
        => CacheObject.Compounds[syntax].View[accessPosition ?? syntax.EndPosition];

    internal Compound GetCompound(CompoundSyntax context) => CacheObject.Compounds[context];

    //[DebuggerHidden]
    internal Result GetResult(Category category, ValueSyntax syntax)
        => GetResultCache(syntax).Get(category);

    internal ResultCache GetResultCache(ValueSyntax syntax)
        => CacheObject.ResultCache[syntax];

    internal ResultCache GetResultAsReferenceCache(ValueSyntax syntax)
        => CacheObject.ResultAsReferenceCache[syntax];

    internal TypeBase GetTypeIfKnown(ValueSyntax syntax)
        => CacheObject.ResultCache[syntax].Data.Type;

    //[DebuggerHidden]
    Result GetResultForCache(Category category, ValueSyntax syntax)
    {
        var trace = syntax.ObjectId.In() && ObjectId.In(7) && category.HasType();
        StartMethodDump(trace, category, syntax);
        try
        {
            BreakExecution();
            var result = syntax.GetResultForCache(this, category.Replenished());
            (result == null || result.IsValidOrIssue(category)).Assert();
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    [DebuggerHidden]
    ResultCache GetResultCacheForCache(ValueSyntax syntax)
    {
        var result = new ResultCache(new ResultProvider(this, syntax));
        syntax.AddToCacheForDebug(this, result);
        return result;
    }

    ResultCache GetResultAsReferenceCacheForCache(ValueSyntax syntax) 
        => new(new ResultProvider(this, syntax, true));

    internal Result GetResultAsReference(Category category, ValueSyntax syntax)
        => GetResult(category | Category.Type, syntax)
            .LocalReferenceResult;

    internal Result GetArgReferenceResult(Category category)
        => FindRecentFunctionContextObject
            .CreateArgumentReferenceResult(category);

    /// <summary>
    ///     Obtains the feature result of a functional argument object.
    ///     Actual arguments, if provided, as well as object reference are replaced.
    /// </summary>
    /// <param name="category"> the categories in result </param>
    /// <param name="right"> the expression of the argument of the call. Must not be null </param>
    /// <param name="token"></param>
    /// <returns> </returns>
    internal Result GetFunctionalArgResult(Category category, ValueSyntax right, SourcePart token)
    {
        var argsType = FindRecentFunctionContextObject.ArgumentsType;
        return argsType
            .GetResult
            (
                category,
                new(GetFunctionalArgObjectResult),
                token,
                null,
                this,
                right
            );
    }

    Result GetFunctionalArgObjectResult(Category category)
    {
        NotImplementedMethod(category);
        return null;
    }

    IImplementation GetDeclaration(Definable tokenClass)
    {
        var genericTokenClass = tokenClass.MakeGeneric.ToArray();
        var results
            = genericTokenClass
                .SelectMany(g => g.GetDeclarations(this));
        var result = results.SingleOrDefault();
        if(result != null || RootContext.ProcessErrors)
            return result;

        NotImplementedMethod(tokenClass);
        return null;
    }

    internal Result GetPrefixResult
        (Category category, Definable definable, SourcePart token, ValueSyntax right)
    {
        var searchResult = GetDeclaration(definable);
        if(searchResult == null)
            return IssueId
                .MissingDeclarationInContext
                .GetResult(category, token, this);

        var result = searchResult.GetResult(category, CacheObject.AsObject, token, this, right);

        (result.HasIssue || result.CompleteCategory.Contains(category)).Assert();
        return result;
    }

    public Result CreateArrayResult(Category category, ValueSyntax argsType, bool isMutable)
    {
        var target = GetResult(category | Category.Type, argsType).GetSmartUn<PointerType>().Align;
        return target
                .Type
                .GetArray(1, ArrayType.Options.Create().IsMutable.SetTo(isMutable))
                .GetResult(category | Category.Type, target)
                .LocalReferenceResult
            & category;
    }
}

sealed class SmartNodeAttribute : Attribute { }