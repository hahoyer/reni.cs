using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class DefinableTokenSyntax : ParsedSyntax
    {
        private readonly Defineable _defineable;

        internal DefinableTokenSyntax(Defineable defineable, TokenData tokenData)
            : base(tokenData) { _defineable = defineable; }

        protected override TokenData GetFirstToken() { return Token; }
        protected override TokenData GetLastToken() { return Token; }
        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right) { return new DeclarationSyntax(_defineable, token, right); }
        internal override ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return this; }
        internal override ICompileSyntax ToCompiledSyntax() { return new ExpressionSyntax(_defineable, null, Token, null); }
    }
}