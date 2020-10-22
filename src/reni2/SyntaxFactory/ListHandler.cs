using System.Linq;
using hw.DebugFormatter;
using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    class ListHandler : DumpableObject, IStatementsProvider
    {
        Result<IStatementSyntax[]> IStatementsProvider.Get(BinaryTree leftAnchor, BinaryTree target, Factory factory)
            => (
                    factory.GetStatementsSyntax(leftAnchor, target.Left, null),
                    factory.GetStatementsSyntax(target, target.Right, null)
                )
                .Apply((left, right) => left.Concat(right).ToArray());
    }
}