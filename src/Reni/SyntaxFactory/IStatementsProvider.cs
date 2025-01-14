using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory;

interface IStatementsProvider
{
    IStatementSyntax[] Get(BinaryTree? target, Factory factory, Anchor frameItems = null);
}