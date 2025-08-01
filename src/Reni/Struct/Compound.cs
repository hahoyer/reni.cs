using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.FeatureTest.Helper;
using Reni.Helper;
using Reni.SyntaxTree;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

sealed class Compound
    : DumpableObject, IContextReference, IChild<ContextBase>, ValueCache.IContainer

{
    static int NextObjectId;

    [Node]
    [DisableDump]
    internal TokenClasses.Brackets.Setup BracketsSetup => Syntax.BracketsSetup ;

    [Node]
    [DisableDump]
    internal readonly ContextBase Parent;

    [Node]
    internal readonly CompoundSyntax Syntax;

    [DisableDump]
    internal readonly FunctionCache<int, CompoundView> View;

    readonly int Order = Closures.NextOrder++;

    [DisableDump]
    internal TypeBase IndexType => Parent.RootContext.BitType.Number(IndexSize.ToInt());

    [DisableDump]
    internal Root Root => Parent.RootContext;

    CompoundView CompoundView => Parent.GetCompoundView(Syntax);

    Size IndexSize => Syntax.IndexSize;

    [DisableDump]
    [UsedImplicitly]
    internal IEnumerable<ResultCache> CachedResults => Syntax.EndPosition.Select(CachedResult);

    int EndPosition => Syntax.EndPosition;

    [DisableDump]
    [UsedImplicitly]
    internal bool HasIssue => Issues.Any();

    [DisableDump]
    [PublicAPI]
    internal Issue[] Issues => GetIssues();

    internal Compound(CompoundSyntax syntax, ContextBase parent)
        : base(NextObjectId++)
    {
        Syntax = syntax;
        Parent = parent;
        View = new(position => new(this, position));
        StopByObjectIds();
    }

    [DisableDump]
    ContextBase IChild<ContextBase>.Parent => Parent;

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    int IContextReference.Order { get; } = Closures.NextOrder++;

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + Syntax.GetCompoundIdentificationDump() + ")";

    ResultCache CachedResult(int position)
        => Parent
            .GetCompoundPositionContext(Syntax, position)
            .GetResultCache(Syntax.PureStatements[position]);

    internal Size Size(int? position = null)
    {
        if(IsHollow(position))
            return Basics.Size.Zero;
        return ResultsOfStatements(Category.Size, 0, position).Size;
    }

    internal bool IsHollow(int? accessPosition = null) => ObtainIsHollow(accessPosition);

    internal Size FieldOffsetFromAccessPoint(int accessPosition, int fieldPosition)
        => ResultsOfStatements(Category.Size, fieldPosition + 1, accessPosition).Size;

    Result ResultsOfStatements(Category category, int fromPosition, int? fromNotPosition)
    {
        if(category == Category.None)
            return new(category);
        var trace = Syntax.ObjectId.In() && category.HasCode();
        StartMethodDump(trace, category, fromPosition, fromNotPosition);
        try
        {
            Dump(nameof(Syntax.PureStatements), Syntax.PureStatements);
            BreakExecution();

            var positions = ((fromNotPosition ?? EndPosition) - fromPosition)
                .Select(i => fromPosition + i)
                .Where(position => Syntax.PureStatements[position].IsLambda == false)
                .ToArray();

            var rawResults = positions
                .Select(position => AccessResult(category, position))
                .ToArray();

            var results = rawResults
                .Select(r => r.Aligned.GetLocalBlock(category))
                .ToArray();
            Dump(nameof(results), results);
            BreakExecution();
            var result = results
                .Aggregate
                (
                    Root.VoidType.GetResult(category | Category.Type),
                    (current, next) => current.GetSequence(next)
                );
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal Result GetResult(Category category)
    {
        var trace = Syntax.ObjectId.In() && category.HasType();
        StartMethodDump(trace, category);
        try
        {
            BreakExecution();
            var resultsOfStatements = ResultsOfStatements
                (category.Without(Category.Type), 0, Syntax.EndPosition);

            Dump("resultsOfStatements", resultsOfStatements);
            BreakExecution();

            var aggregate = Syntax
                .EndPosition
                .Select()
                .Aggregate(resultsOfStatements, Combine);

            Dump(nameof(aggregate), aggregate);
            BreakExecution();

            var resultWithCleanup = aggregate.ArrangeCleanupCode();
            Dump(nameof(resultWithCleanup), resultWithCleanup);
            BreakExecution();

            var result = resultWithCleanup
                    .ReplaceRelative(this, CodeBase.GetTopRef, Closures.GetVoid)
                & category;

            if(result.HasIssue)
                return ReturnMethodDump(result);

            if(category.HasType())
                result.Type = CompoundView.Type;

            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    Result Combine(Result result, int position)
        => Parent.GetCompoundView(Syntax, position).ReplaceObjectPointerByContext(result);

    Result AccessResult(Category category, int position)
    {
        (!Syntax.PureStatements[position].IsLambda).Assert();
        return AccessResult(category, position, position);
    }

    Result AccessResult(Category category, int accessPosition, int position)
    {
        var trace = ObjectId == -19
                && Syntax.ObjectId.In(13)
                && accessPosition == 2
                && position == 1
                //&& category.HasClosures()
            ;
        StartMethodDump(trace, category, accessPosition, position);
        try
        {
            var uniqueChildContext = Parent.GetCompoundPositionContext(Syntax, accessPosition);
            Dump(nameof(Syntax.PureStatements), Syntax.PureStatements[position]);
            BreakExecution();
            var rawResult = uniqueChildContext.GetResult
                (category | Category.Type, Syntax.PureStatements[position]);
            Dump(nameof(rawResult), rawResult);
            rawResult.CompleteCategory.Contains(category | Category.Type).Assert();
            BreakExecution();
            var unFunction = rawResult.GetSmartUn<FunctionType>();
            Dump(nameof(unFunction), unFunction);
            BreakExecution();
            var result = unFunction.AutomaticDereference;
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal TypeBase AccessType(int accessPosition, int position)
        => AccessResult(Category.Type, accessPosition, position).Type;

    bool ObtainIsHollow(int? accessPosition)
    {
        var trace = ObjectId == -10 && accessPosition == 3 && Parent.ObjectId == 4;
        StartMethodDump(trace, accessPosition);
        try
        {
            var subStatementIds = (accessPosition ?? EndPosition).Select().ToArray();
            Dump("subStatementIds", subStatementIds);
            BreakExecution();
            if(subStatementIds.Any(position => InnerIsHollowStatic(position) == false))
                return ReturnMethodDump(false);
            var quickNonDataLess = subStatementIds
                .Where(position => InnerIsHollowStatic(position) == null)
                .ToArray();
            Dump("quickNonDataLess", quickNonDataLess);
            BreakExecution();
            if(quickNonDataLess.Length == 0)
                return ReturnMethodDump(true);
            if(quickNonDataLess.Any
                   (position => InternalInnerIsHollowStructureElement(position) == false))
                return ReturnMethodDump(false);
            return ReturnMethodDump(true);
        }
        finally
        {
            EndMethodDump();
        }
    }

    bool InternalInnerIsHollowStructureElement(int position)
    {
        var uniqueChildContext = Parent
            .GetCompoundPositionContext(Syntax, position);
        return Syntax
            .PureStatements[position]
            .GetIsHollowStructureElement(uniqueChildContext);
    }

    bool? InnerIsHollowStatic(int position)
    {
        var statement = Syntax.PureStatements[position];
        // if there is no statement (f.i. because of errors) it is granted to be hollow.
        return statement.IsHollow;
    }

    internal Result Cleanup(Category category)
    {
        var uniqueChildContext = Parent.GetCompoundPositionContext(Syntax);
        var cleanup = Syntax.GetCleanup(uniqueChildContext, category);
        var aggregate = EndPosition
            .Select()
            .Reverse()
            .Select(index => GetCleanup(category, index))
            .Aggregate();
        return (cleanup + aggregate)
            .ExpectNotNull();
    }

    Result GetCleanup(Category category, int index)
    {
        var result = AccessType(EndPosition, index).GetCleanup(category);
        result.CompleteCategory.Contains(category).Assert();
        return result;
    }

    internal Issue[] GetIssues(int? viewPosition = null)
        => ResultsOfStatements(Category.Type, 0, viewPosition).Issues;

}