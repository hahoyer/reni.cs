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
            var left = factory.GetStatementsSyntax(target.Left, anchor?.Left);
            var right = factory.GetStatementsSyntax(target.Right, anchor?.Right);
            return left.Concat(right).ToArray();
        }
    }
}