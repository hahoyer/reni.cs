using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        Result<StatementSyntax[]> IStatementsProvider.Get(BinaryTree anchor, BinaryTree target, Factory factory)
            => (
                    factory.GetStatementsSyntax(anchor, target.Left),
                    factory.GetStatementsSyntax(target, target.Right)
                )
                .Apply((left, right) => left.Concat(right).ToArray());
    }
}