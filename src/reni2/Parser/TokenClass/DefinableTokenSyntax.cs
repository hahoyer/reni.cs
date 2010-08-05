using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    [Serializable]
    internal sealed class DefinableTokenSyntax : ParsedSyntax
    {
        private readonly DefineableToken _defineableToken;

        public DefinableTokenSyntax(Token token)
            : base(token) { _defineableToken = new DefineableToken(token); }

        protected override IParsedSyntax CreateDeclarationSyntax(Token token, IParsedSyntax right) { return new DeclarationSyntax(_defineableToken, token, right); }
        protected override Token GetFirstToken() { return Token; }
        protected override Token GetLastToken() { return Token; }
        protected override IParsedSyntax SurroundedByParenthesis(Token leftToken, Token rightToken) { return this; }
        protected override ICompileSyntax ToCompiledSyntax() { return new ExpressionSyntax(null, Token, null); }
    }
}