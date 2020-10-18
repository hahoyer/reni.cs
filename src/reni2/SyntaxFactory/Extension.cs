using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    static class Extension
    {
        internal static Result<ValueSyntax> EmptyListIfNull(this BinaryTree target) => new EmptyList(target);

        internal static Result<DeclarerSyntax> Combine
        (
            this Result<DeclarerSyntax> target,
            Result<DeclarerSyntax> other,
            BinaryTree root = null
        )
            => (target, other)
                .Apply((target, other) => target?.Combine(other) ?? other);

        internal static Result<ValueSyntax> GetInfixSyntax
        (
            this ValueSyntax left,
            BinaryTree target,
            ValueSyntax right
        )
            => left == null
                ? right == null
                    ? (Result<ValueSyntax>)new TerminalSyntax((ITerminal)target.TokenClass, target)
                    : new PrefixSyntax((IPrefix)target.TokenClass, right, target)
                : right == null
                    ? (Result<ValueSyntax>)new SuffixSyntax(left, (ISuffix)target.TokenClass, target)
                    : new InfixSyntax(left, (IInfix)target.TokenClass, right, target);
    }
}