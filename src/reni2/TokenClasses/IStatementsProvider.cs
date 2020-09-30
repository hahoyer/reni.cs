using Reni.Parser;

namespace Reni.TokenClasses
{
    interface IStatementsProvider
    {
        Result<Statement[]> Get(List type, Syntax syntax, IDefaultScopeProvider container);
    }
}