using hw.DebugFormatter;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

[BelongsTo(typeof(MainTokenFactory))]
sealed class ArrayInstanceToken : InfixSyntaxToken, IPendingProvider, IRecursionHandler
{
    public const string TokenId = "array_instance";

    Result IPendingProvider.Result
        (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
    {
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
            .ArrayInstanceResult(category, c => context.ResultAsReference(c, right));
    }
}