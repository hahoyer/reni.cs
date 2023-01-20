using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;

namespace Reni.TokenClasses;

sealed class FunctionInstanceToken : SuffixSyntaxToken
{
    public const string TokenId = "function_instance";
    public override string Id => TokenId;

    protected override Result Result(ContextBase context, Category category, ValueSyntax left)
        => context.GetResult(category | Category.Type, left)
            .Type
            .FunctionInstance
            .Result(category, context.GetResult(category | Category.Type, left));
}