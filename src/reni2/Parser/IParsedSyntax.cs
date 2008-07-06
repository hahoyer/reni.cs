using HWClassLibrary.Debug;
using Reni.Syntax;

namespace Reni.Parser
{
    internal interface IParsedSyntax
    {
        string Dump();
        string DumpShort();
        IParsedSyntax SurroundedByParenthesis(Token token);
        IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right);
        [DumpData(false)]
        ICompileSyntax ToCompileSyntax { get; }
        IParsedSyntax CreateThenSyntax(Token token, ICompileSyntax condition);
    }
}