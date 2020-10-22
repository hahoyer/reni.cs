using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class CleanupHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider
            .Get
            (
                BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory
                , FrameItemContainer frameItems
            )
            => (
                    factory.GetStatementsSyntax(leftAnchor, target.Left, FrameItemContainer.Create()),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply
                ((statements, cleanup)
                    => (ValueSyntax)CompoundSyntax.Create(statements,
                        new CleanupSyntax(target, cleanup),
                        frameItems)
                );
    }
}