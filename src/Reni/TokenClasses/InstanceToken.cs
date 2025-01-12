#nullable enable
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class InstanceToken : InfixSyntaxToken, IPendingProvider
{
    [PublicAPI]
    internal const string TokenId = "instance";

    Result? IPendingProvider.GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
        if(Category.Type.Replenished().Contains(category))
            return GetResult(context, category, left, right);

        NotImplementedMethod(context, category, left, right);
        return null;
    }

    public override string Id => TokenId;

    protected override Result GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right) 
        => left.GetTypeBase(context)
        .GetInstanceResult(category, c => context.GetResultAsReference(c, right));
}