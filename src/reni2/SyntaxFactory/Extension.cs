using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    static class Extension
    {
        internal static ValueSyntax GetInfixSyntax
        (
            this ValueSyntax left,
            BinaryTree target,
            ValueSyntax right, FrameItemContainer frameItems
        )
            => left == null
                ? right == null
                    ? (ValueSyntax)new TerminalSyntax((ITerminal)target.TokenClass, target, frameItems)
                    : new PrefixSyntax((IPrefix)target.TokenClass, right, target, frameItems)
                : right == null
                    ? (ValueSyntax)new SuffixSyntax(left, (ISuffix)target.TokenClass, target, frameItems)
                    : new InfixSyntax(left, (IInfix)target.TokenClass, right, target, frameItems);
    }
}