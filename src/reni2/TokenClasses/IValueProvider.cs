using Reni.Parser;

namespace Reni.TokenClasses
{
    interface IValueProvider
    {
        Result<ValueSyntax> Get(BinaryTree binaryTree, ISyntaxScope scope);
    }
}