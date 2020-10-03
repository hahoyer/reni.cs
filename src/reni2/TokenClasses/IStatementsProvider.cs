using Reni.Parser;

namespace Reni.TokenClasses
{
    interface IStatementsProvider
    {
        Result<Statement[]> Get(List type, BinaryTree binaryTree, IValuesScope scope);
    }
}