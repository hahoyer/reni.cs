using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    interface ITokenClass
    {
        string Id { get; }
        Checked<CompileSyntax> ToCompiledSyntax(SourceSyntax left, IToken token, SourceSyntax right);
    }
}