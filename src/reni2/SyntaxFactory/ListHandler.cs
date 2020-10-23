using System.Linq;
using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        IStatementSyntax[] IStatementsProvider.Get(BinaryTree target, Factory factory, FrameItemContainer frameItems)
        {
            var left = factory.GetStatementsSyntax(target.Left, frameItems);
            var right = factory.GetStatementsSyntax(target.Right, FrameItemContainer.Create(target));
            return left.Concat(right).ToArray();
        }
    }
}