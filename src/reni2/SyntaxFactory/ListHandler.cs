using System.Linq;
using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        IStatementSyntax[] IStatementsProvider.Get(BinaryTree target, Factory factory, Anchor frameItems)
        {
            var left = factory.GetStatementsSyntax(target.Left, frameItems);
            var right = factory.GetStatementsSyntax(target.Right, Anchor.Create(target));
            return left.Concat(right).ToArray();
        }
    }
}