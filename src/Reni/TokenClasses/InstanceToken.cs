using hw.DebugFormatter;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class InstanceToken : InfixSyntaxToken, IPendingProvider, IRecursionHandler
{
    public const string TokenId = "instance";

    Result IPendingProvider.Result
        (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
        if(Category.Type.Replenished().Contains(category))
            return Result(context, category, left, right);

        NotImplementedMethod(context, category, left, right);
        return null;
    }

    Result IRecursionHandler.Execute
    (
        ContextBase context,
        Category category,
        Category pendingCategory,
        ValueSyntax syntax,
        bool asReference
    )
    {
        if(!asReference && Category.Type.Contains(category | pendingCategory))
            return syntax.ResultForCache(context, Category.Type);

        NotImplementedMethod(context, category, pendingCategory, syntax, asReference);
        return null;
    }

    public override string Id => TokenId;

    protected override Result Result
        (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
        var leftType = left.Type(context);
        (leftType != null).Assert();
        return leftType
            .InstanceResult(category, c => context.GetResultAsReference(c, right));
    }
}