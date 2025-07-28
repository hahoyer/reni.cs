using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.Struct;

[BelongsTo(typeof(MainTokenFactory))]
[UsedImplicitly]
sealed class AtToken : InfixPrefixSyntaxToken
{
    public const string TokenId = "_A_T_";
    public override string Id => TokenId;

    protected override Result GetResult
        (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
        var target = context.GetResultAsReference(category | Category.Type, left);
        return target
            .Type
            .AssertNotNull().FindRecentCompoundView()
            .AccessViaPositionExpression(category, right.GetResultForAll(context))
            .ReplaceArguments(target);
    }

    protected override Result GetResult(ContextBase context, Category category, ValueSyntax right)
        => context.FindRecentCompoundView.AtTokenResult(category, right.GetResultForAll(context));
}