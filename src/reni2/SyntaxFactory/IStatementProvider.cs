using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IStatementProvider
    {
        Result<IStatementSyntax> Get(BinaryTree target, Factory factory);
    }
}