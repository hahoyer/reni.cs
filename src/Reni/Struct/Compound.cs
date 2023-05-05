using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.SyntaxTree;
using Reni.Type;
using Reni.Validation;

namespace Reni.Struct;

sealed class Compound
    : DumpableObject, IContextReference, IChild<ContextBase>, ValueCache.IContainer, IRootProvider

{
    static int NextObjectId;

    [Node]
    [DisableDump]
    internal readonly ContextBase Parent;

    [Node]
    internal readonly CompoundSyntax Syntax;

    [DisableDump]
    internal readonly FunctionCache<int, CompoundView> View;

    readonly int Order;

    [DisableDump]
    internal TypeBase IndexType => Parent.RootContext.BitType.Number(IndexSize.ToInt());

    [DisableDump]
    internal Root Root => Parent.RootContext;

    [DisableDump]
    internal CompoundView CompoundView => Parent.GetCompoundView(Syntax);

    Size IndexSize => Syntax.IndexSize;

    [DisableDump]
    internal IEnumerable<ResultCache> CachedResults => Syntax.EndPosition.Select(CachedResult);

    int EndPosition => Syntax.EndPosition;

    [DisableDump]
    public bool HasIssue => Issues?.Any() ?? false;

    [DisableDump]
    public Issue[] Issues => GetIssues();

    internal Compound(CompoundSyntax syntax, ContextBase parent)
        : base(NextObjectId++)
    {
        Order = Closures.NextOrder++;
        Syntax = syntax;
        Parent = parent;
        View = new(position => new(this, position));
        StopByObjectIds();
    }

    [DisableDump]
    ContextBase IChild<ContextBase>.Parent => Parent;

    ValueCache ValueCache.IContainer.Cache { get; } = new();

    int IContextReference.Order => Order;

    Root IRootProvider.Value => Root;

    protected override string GetNodeDump()
        => base.GetNodeDump() + "(" + GetCompoundIdentificationDump() + ")";

    public string GetCompoundIdentificationDump() => Syntax.GetCompoundIdentificationDump();

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
                .Where(position => Syntax.PureStatements[position]?.IsLambda == false)
                .ToArray();

            var rawResults = positions
                .Select(position => AccessResult(category, position))
                .ToArray();

            var results = rawResults
                .Select(r => r.Align.GetLocalBlock(category))
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
        var trace = ObjectId == -10
                && Syntax.ObjectId.In(1)
                && accessPosition == 1
                && position == 1
                && category.HasClosures()
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
            var result = unFunction.AutomaticDereferenceResult;
            return ReturnMethodDump(result);
        }
        finally
        {
            EndMethodDump();
        }
    }

    internal TypeBase AccessType(int accessPosition, int position)
        => AccessResult(Category.Type, accessPosition, position).Type.AssertNotNull();

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
        return statement == null? true : statement.IsHollow;
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
        return cleanup + aggregate;
    }

    Result GetCleanup(Category category, int index)
    {
        var result = AccessType(EndPosition, index).GetCleanup(category);
        (result.CompleteCategory .Contains(category)).Assert();
        return result;
    }

    internal Issue[] GetIssues(int? viewPosition = null)
        => ResultsOfStatements(Category.Type, 0, viewPosition)
            .Issues;
}