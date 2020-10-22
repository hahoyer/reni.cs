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
                , FrameItemContainer brackets
            )
        {
            NotImplementedMethod(leftAnchor, target, rightAnchor, factory);

            return (
                    factory.GetStatementsSyntax(leftAnchor, target.Left, null),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply
                ((statements, cleanup)
                    => (ValueSyntax)CompoundSyntax.Create(statements,
                        new CleanupSyntax(target, cleanup),
                        null)
                );
        }

        UsageTree IValueProvider.GetUsage
            (BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => new UsageTree {Right = true};
    }
}