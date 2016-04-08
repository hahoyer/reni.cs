using hw.Parser;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Parser
{
    interface ITokenClass
    {
        string Id { get; }
        Checked<Value> GetValue(Syntax left, SourcePart token, Syntax right);
    }
}