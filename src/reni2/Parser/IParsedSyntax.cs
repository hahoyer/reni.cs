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

        Token FirstToken { get; }
        Token LastToken { get; }

        ICompileSyntax ToCompiledSyntax();
        IParsedSyntax SurroundedByParenthesis(Token token, Token token1);
        IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right);
        IParsedSyntax CreateThenSyntax(Token token, ICompileSyntax condition);
        IParsedSyntax CreateSyntaxOrDeclaration(Token token, IParsedSyntax right);
        IParsedSyntax CreateElseSyntax(Token token, ICompileSyntax elseSyntax);
        IParsedSyntax RightPar(Token token);
    }
}