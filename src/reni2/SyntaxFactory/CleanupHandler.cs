using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class CleanupHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider.Get(
            BinaryTree target, Factory factory, FrameItemContainer frameItems
            )
            => (
                    factory.GetStatementsSyntax(target.Left, FrameItemContainer.Create()),
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