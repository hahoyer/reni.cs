using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IStatementsProvider
    {
        Result<IStatementSyntax[]> Get
            (BinaryTree leftAnchor, BinaryTree target, Factory factory, FrameItemContainer brackets);
    }
}