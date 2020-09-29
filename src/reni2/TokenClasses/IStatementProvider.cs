using Reni.Parser;

namespace Reni.TokenClasses
{
    interface IStatementProvider
    {
        Result<Statement> Get(Syntax left, Syntax right, IDefaultScopeProvider container);
    }
}