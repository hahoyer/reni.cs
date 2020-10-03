using Reni.Parser;

namespace Reni.TokenClasses
{
    interface IValueProvider
    {
        Result<Syntax> Get(BinaryTree binaryTree, IValuesScope scope);
    }
}