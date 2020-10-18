using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IDeclarerProvider
    {
        Result<DeclarerSyntax> Get(BinaryTree target, Factory factory);
    }
}