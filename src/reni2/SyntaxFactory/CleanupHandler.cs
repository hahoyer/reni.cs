using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class CleanupHandler : DumpableObject, IValueProvider
    {
        ValueSyntax IValueProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
        {
            var statements = factory.GetStatementsSyntax(target.Left, FrameItemContainer.Create());
            var cleanup = factory.GetValueSyntax(target.Right);
            return CompoundSyntax.Create(statements, new CleanupSyntax(cleanup), frameItems);
        }
    }
}