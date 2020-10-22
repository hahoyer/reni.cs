using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IStatementsProvider
    {
        Result<StatementSyntax[]> Get(BinaryTree leftAnchor, BinaryTree target, Factory factory);
    }
}