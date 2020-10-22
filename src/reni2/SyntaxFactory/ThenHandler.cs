using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ThenHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get
        (
            BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory
            , FrameItemContainer frameItems
        )
            => (
                    factory.GetValueSyntax(target.Left),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply((condition, thenClause)
                    => (ValueSyntax)new CondSyntax(condition, thenClause, null, target, frameItems));
    }
}