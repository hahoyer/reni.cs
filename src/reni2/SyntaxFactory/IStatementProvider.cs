using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IStatementProvider
    {
        IStatementSyntax Get(BinaryTree target, Factory factory, Anchor anchor);
    }
}