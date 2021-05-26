using System.Linq;
using hw.DebugFormatter;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        IStatementSyntax[] IStatementsProvider.Get(BinaryTree target, Factory factory, Anchor anchor)
        {
            (anchor == null || anchor.IsEmpty).Assert();
            var left = factory.GetStatementsSyntax(target.Left, anchor?.GetLeftOf(target), target.TokenClass);
            var right = factory.GetStatementsSyntax(target.Right, anchor?.GetRightOf(target), target.TokenClass);
            return left.Concat(right).ToArray();
        }
    }
}