using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    sealed class CleanupHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, Anchor frameAnchor)
        {
            var statements = factory.GetStatementsSyntax(target.Left, null, target.Left?.TokenClass);
            var cleanup = factory.GetValueSyntax(target.Right);
            var cleanupSection = new CleanupSyntax(cleanup, Anchor.Create(target));
            var anchor = Anchor.Create(target.Left?.ParserLevelGroup).Combine(frameAnchor);
            return CompoundSyntax.Create(statements, cleanupSection, anchor);
        }
    }
}