using hw.DebugFormatter;
using hw.Parser;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class InstanceToken : InfixSyntaxToken, IPendingProvider, IRecursionHandler
{
    [PublicAPI]
    internal const string TokenId = "instance";

    Result IPendingProvider.GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
        if(Category.Type.Replenished().Contains(category))
            return GetResult(context, category, left, right);

        NotImplementedMethod(context, category, left, right);
        return null;
    }

    Result IRecursionHandler.Execute
    (
        ContextBase context,
        Category category,
        ValueSyntax syntax,
        bool asReference
    )
    {
        if(!asReference && Category.Type.Contains(category))
            return syntax.GetResultForCache(context, Category.Type);

        NotImplementedMethod(context, category, syntax, asReference);
        return null;
    }

    public override string Id => TokenId;

    protected override Result GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
        var leftType = left.Type(context);
        (leftType != null).Assert();
        return leftType
            .GetInstanceResult(category, c => context.GetResultAsReference(c, right));
    }
}