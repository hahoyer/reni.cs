using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Syntax;

namespace Reni.Parser
{
    internal interface IParsedSyntax : IIconKeyProvider
    {
        string Dump();
        string DumpShort();

        [DumpData(false)]
        Token Token { get; }

        ICompileSyntax ToCompiledSyntax();
        IParsedSyntax SurroundedByParenthesis(Token token);
        IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right);
        IParsedSyntax CreateThenSyntax(Token token, ICompileSyntax condition);
        IParsedSyntax CreateSyntaxOrDeclaration(Token token, IParsedSyntax right);
        IParsedSyntax CreateElseSyntax(Token token, ICompileSyntax elseSyntax);
    }
}