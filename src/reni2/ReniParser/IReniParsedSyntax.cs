using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Syntax;

namespace Reni.ReniParser
{
    [Obsolete] internal interface IReniParsedSyntax : IParsedSyntax
    {
        ICompileSyntax ToCompiledSyntax();
        IReniParsedSyntax CreateDeclarationSyntax(TokenData token, IReniParsedSyntax right);
        IReniParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, IReniParsedSyntax right);
        IReniParsedSyntax CreateThenSyntax(TokenData token, ICompileSyntax condition);
        IReniParsedSyntax CreateElseSyntax(TokenData token, ICompileSyntax elseSyntax);
    }
}