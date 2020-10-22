using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class MatchedBracketHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get
        (
            BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory
            , FrameItemContainer brackets
        )
            => (
                    factory.GetValueSyntax(target.Left),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply((left, right)
                    => (ValueSyntax)new ExpressionSyntax
                    (
                        leftAnchor,
                        target,
                        left,
                        null,
                        right,
                        rightAnchor, null)
                );

        UsageTree IValueProvider.GetUsage(BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => new UsageTree();
    }
}