using Reni.Basics;
using Reni.Context;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.TokenClasses;

sealed class FunctionInstanceToken : SuffixSyntaxToken
{
    public const string TokenId = "function_instance";
    public override string Id => TokenId;

    protected override Result GetResult(ContextBase context, Category category, ValueSyntax left)
        => context.GetResult(category | Category.Type, left)
            .Type
            .FunctionInstance
            .GetResult(category, context.GetResult(category | Category.Type, left));

    protected override TypeBase TryGetTypeBase(ValueSyntax left)
    {
        NotImplementedMethod(left);
        return default;
    }
}