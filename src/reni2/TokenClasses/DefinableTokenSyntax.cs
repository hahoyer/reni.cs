using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class DefinableTokenSyntax : ParsedSyntax
    {
        readonly Definable _definable;

        internal DefinableTokenSyntax(Definable definable, TokenData tokenData)
            : base(tokenData)
        {
            _definable = definable;
        }

        [DisableDump]
        internal override TokenData FirstToken { get { return Token; } }
        [DisableDump]
        internal override TokenData LastToken { get { return Token; } }

        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right)
        {
            return new DeclarationSyntax(_definable, token, right);
        }
        internal override ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return this; }
        internal override CompileSyntax ToCompiledSyntax() { return new ExpressionSyntax(_definable, null, Token, null); }
    }
}