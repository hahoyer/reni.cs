using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class CleanupHandler : DumpableObject, IValueProvider
    {
        Result<ValueSyntax> IValueProvider
            .Get(BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory)
            =>
                (
                    factory.GetStatementsSyntax(leftAnchor, target.Left),
                    factory.GetValueSyntax(target.Right)
                )
                .Apply
                ((statements, cleanup)
                    => (ValueSyntax)new CompoundSyntax
                    (
                        statements,
                        new CleanupSyntax(target, cleanup),
                        rightAnchor
                    )
                );

        UsageTree IValueProvider.GetUsage
            (BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => new UsageTree {Right = true};
    }
}