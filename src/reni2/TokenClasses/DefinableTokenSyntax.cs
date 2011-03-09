using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class DefinableTokenSyntax : ReniParser.ParsedSyntax
    {
        private readonly Defineable _defineable;

        internal DefinableTokenSyntax(Defineable defineable, TokenData tokenData)
            : base(tokenData) { _defineable = defineable; }

        protected override TokenData GetFirstToken() { return Token; }
        protected override TokenData GetLastToken() { return Token; }
        internal override ReniParser.ParsedSyntax CreateDeclarationSyntax(TokenData token, ReniParser.ParsedSyntax right) { return new DeclarationSyntax(_defineable, token, right); }
        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return this; }
        internal override ICompileSyntax ToCompiledSyntax() { return new ExpressionSyntax(_defineable, null, Token, null); }
    }
}