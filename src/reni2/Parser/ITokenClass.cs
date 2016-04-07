using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    interface ITokenClass
    {
        string Id { get; }
        Checked<Value> GetValue(Syntax left, IToken token, Syntax right);
    }
}