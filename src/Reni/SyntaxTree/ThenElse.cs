#nullable enable
using Reni.Basics;
using Reni.Context;
using Reni.Helper;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.SyntaxTree;

sealed class CondSyntax : ValueSyntax
{
    [Node]
    [EnableDump]
    readonly ValueSyntax Cond;

    [Node]
    [EnableDump]
    readonly ValueSyntax? Else;

    [Node]
    [EnableDump]
    readonly ValueSyntax? Then;

    internal CondSyntax
        (ValueSyntax condSyntax, ValueSyntax? thenSyntax, ValueSyntax? elseSyntax, Anchor anchor)
        : base(anchor)
    {
        Cond = condSyntax;
        Then = thenSyntax;
        Else = elseSyntax;
        AssertValid();
    }

    internal override void AssertValid(Level? level = null, BinaryTree? target = null)
    {
        Cond.ExpectIsNotNull(() => (Anchor.Main.FullToken, null));
        base.AssertValid(level, target);
    }

    protected override int DirectChildCount => 3;

    protected override Syntax? GetDirectChild(int index)
        => index switch
        {
            0 => Cond, 1 => Then, 2 => Else, _ => null
        };

    internal override Result? GetResultForCache(ContextBase context, Category category)
        => InternalResult(context, category);

    Result? CondResult(ContextBase context, Category category)
        => context.GetResult(category | Category.Type, Cond)?
            .GetConversion(context.RootContext.BitType.Align)
            .GetLocalBlock(category | Category.Type)
            .GetConversion(context.RootContext.BitType);

    Result? ElseResult(ContextBase context, Category category)
    {
        if(Else == null)
            return Root.VoidType.GetResult(category);

        return BranchResult(context, category, Else);
    }

    Result? ThenResult(ContextBase context, Category category)
        => BranchResult(context, category, Then);

    Result? BranchResult(ContextBase context, Category category, ValueSyntax? syntax)
    {
        var result = syntax == null
            ? Root.VoidType.GetResult(category)
            : context.GetResult(category | Category.Type, syntax);

        if(result == null)
            return null;

        var branchResult = result;
        if(branchResult.HasIssue)
            return branchResult;

        var commonType = CommonType(context);
        return branchResult.Type
                .GetConversion(category | Category.Type, commonType)?
                .ReplaceArguments(branchResult)
            & category;
    }

    Result? InternalResult(ContextBase context, Category category)
    {
        var commonType = CommonType(context);
        if(Category.Type.Replenished().Contains(category))
            return commonType.GetResult(category);

        var branchCategory = category & Category.Code.Replenished();

        var condResult = CondResult(context, category);
        if(condResult == null)
            return null;

        var thenResult = ThenResult(context, branchCategory);
        if(thenResult == null)
            return null;

        var elseResult = ElseResult(context, branchCategory);
        if(elseResult == null)
            return null;

        if(condResult.HasIssue || thenResult.HasIssue)
            return condResult + thenResult + elseResult;
        if(elseResult.HasIssue)
            return condResult + (thenResult + elseResult);

        return commonType
            .GetResult
            (
                category,
                () => condResult.Code.GetThenElse(thenResult.Code, elseResult.Code),
                () => condResult.Closures + thenResult.Closures + elseResult.Closures
            );
    }

    TypeBase CommonType(ContextBase context)
    {
        if(Then == null || Else == null)
            return Root.VoidType;

        var thenType = Then.GetTypeBase(context);
        var elseType = Else.GetTypeBase(context);
        if(thenType == null)
            return elseType?.Align ?? Root.VoidType;
        if(elseType == null)
            return thenType.Align;
        return thenType.GetCommonType(elseType).Align;
    }
}