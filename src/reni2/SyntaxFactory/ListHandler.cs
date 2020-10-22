using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        Result<StatementSyntax[]> IStatementsProvider.Get(BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => (
                    factory.GetStatementsSyntax(leftAnchor, target.Left),
                    factory.GetStatementsSyntax(target, target.Right)
                )
                .Apply((left, right) => left.Concat(right).ToArray());
    }
}