using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature;

[BelongsTo(typeof(MainTokenFactory))]
sealed class TypeOperator : SuffixSyntaxToken
{
    public const string TokenId = "type";
    public override string Id => TokenId;

    protected override Result GetResult(ContextBase context, Category category, ValueSyntax left)
    {
        if(category.HasType())
        {
            var leftType = left.GetTypeBase(context);
            leftType.ExpectIsNotNull(() => (left.Anchor.SourcePart, null));
            if(leftType.HasIssues)
                return new(category, leftType.Issues.AssertNotNull());

            return leftType.Make.TypeForTypeOperator.Make.TypeType
                .GetResult(category);
        }

        return context.RootContext.VoidType.GetResult(category);
    }
}