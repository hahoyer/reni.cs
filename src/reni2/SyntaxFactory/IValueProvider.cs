using Reni.Parser;
using Reni.SyntaxTree;
using Reni.TokenClasses;

namespace Reni.SyntaxFactory
{
    interface IValueProvider
    {
        Result<ValueSyntax> Get
            (BinaryTree leftAnchor, BinaryTree target, BinaryTree rightAnchor, Factory factory);
    }
}