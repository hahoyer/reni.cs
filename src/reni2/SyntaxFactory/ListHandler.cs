using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        Result<IStatementSyntax[]> IStatementsProvider.Get
            (BinaryTree target, Factory factory, FrameItemContainer brackets)
        {
            var left = factory.GetStatementsSyntax(target.Left, brackets);
            var right = factory.GetStatementsSyntax(target.Right, FrameItemContainer.Create(target));
            return left.Concat(right).ToArray();
        }
    }
}