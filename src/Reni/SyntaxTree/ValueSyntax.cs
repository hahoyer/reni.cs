using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.Type;
using Reni.Validation;

namespace Reni.SyntaxTree;

/// <summary>
///     Static syntax items that represent a value
/// </summary>
abstract class ValueSyntax : Syntax, IStatementSyntax
{
    internal new abstract class NoChildren : ValueSyntax
    {
        protected NoChildren(Anchor anchor)
            : base(anchor)
            => anchor.AssertIsNotNull();

        [DisableDump]
        protected sealed override int DirectChildCount => 0;

        protected sealed override Syntax GetDirectChild(int index)
            => throw new($"Unexpected call: {nameof(GetDirectChild)}({index})");
    }

    // Used for debug only
    [DisableDump]
    [Node]
    [UsedImplicitly]
    internal readonly FunctionCache<ContextBase, ResultCache> ResultCache = new();

    [DisableDump]
    internal Issue[] AllIssues => this.CachedValue(GetAllIssues);

    protected ValueSyntax(Anchor anchor)
        : base(anchor)
        => anchor.AssertIsNotNull();

    protected ValueSyntax(int objectId, Anchor anchor)
        : base(anchor, objectId) { }

    DeclarerSyntax IStatementSyntax.Declarer => null;

    SourcePart IStatementSyntax.SourcePart => Anchor.SourceParts.Combine();

    ValueSyntax IStatementSyntax.ToValueSyntax(Anchor anchor) => this;

    ValueSyntax IStatementSyntax.Value => this;

    IStatementSyntax IStatementSyntax.With(Anchor frameItems)
    {
        if(frameItems == null || !frameItems.Items.Any())
            return this;
        NotImplementedMethod(frameItems);
        return default;
    }

    [DisableDump]
    internal virtual bool IsLambda => false;

    [DisableDump]
    internal virtual bool? IsHollow => IsLambda? true : null;

    //[DebuggerHidden]
    internal virtual Result GetResultForCache(ContextBase context, Category category)
    {
        NotImplementedMethod(context, category);
        return null;
    }

    internal virtual ValueSyntax Visit(ISyntaxVisitor visitor)
    {
        NotImplementedMethod(visitor);
        return null;
    }

    internal virtual TypeBase TryGetTypeBase()
    {
        NotImplementedMethod();
        return default;
    }

    Issue[] GetAllIssues() => this
        .GetNodesFromLeftToRight()
        .SelectMany(node => node?.Issues)
        .ToArray();

    internal void AddToCacheForDebug(ContextBase context, ResultCache cacheItem)
        => ResultCache.Add(context, cacheItem);

    internal Result GetResultForAll(ContextBase context) => context.GetResult(Category.All, this);

    internal BitsConst Evaluate(ContextBase context)
        => GetResultForAll(context).GetValue(context.RootContext.ExecutionContext);

    //[DebuggerHidden]
    internal TypeBase GetTypeBase(ContextBase context) => context.GetResult(Category.Type, this)?.Type;

    internal bool GetIsHollowStructureElement(ContextBase context)
    {
        var result = IsHollow;
        if(result != null)
            return result.Value;

        var type = context.GetTypeIfKnown(this);
        return (type ?? GetTypeBase(context)).GetSmartUn<FunctionType>().IsHollow;
    }

    internal ValueSyntax ReplaceArg(ValueSyntax syntax)
        => Visit(new ReplaceArgVisitor(syntax)) ?? this;

    internal IEnumerable<string> GetDeclarationOptions(ContextBase context)
        => GetTypeBase(context).DeclarationOptions;
}