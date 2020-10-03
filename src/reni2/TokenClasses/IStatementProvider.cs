using Reni.Parser;

namespace Reni.TokenClasses
{
    interface IStatementProvider
    {
        Result<Statement> Get(BinaryTree left, BinaryTree right, IValuesScope scope);
    }
}