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
            BinaryTree target, Factory factory
            , FrameItemContainer frameItems
        )
            => (
                    factory.GetValueSyntax(target.Left),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply((left, right)
                    => (ValueSyntax)new ExpressionSyntax
                    (target,
                        left,
                        null,
                        right, frameItems)
                );

    }
}