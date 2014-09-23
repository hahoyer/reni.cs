using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.Type;

namespace Reni.TokenClasses
{
    abstract class Defineable : TokenClass
    {
        protected override sealed ParsedSyntax TerminalSyntax(TokenData token) { return new DefinableTokenSyntax(this, token); }
        protected override sealed ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right)
        {
            return new ExpressionSyntax(this, null, token, (CompileSyntax) right);
        }
        protected override sealed ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token)
        {
            return left.CreateSyntaxOrDeclaration(this, token, null);
        }
        protected override sealed ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            return left.CreateSyntaxOrDeclaration(this, token, right);
        }

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }

        public virtual SearchResult FindGenericDeclarationsForType(TypeBase provider)
        {
            return provider.DeclarationsForType<Defineable>();
        }
    }

    abstract class Defineable<TTarget> : Defineable
        where TTarget : Defineable
    {
        public override SearchResult FindGenericDeclarationsForType(TypeBase provider)
        {
            return provider.DeclarationsForType<TTarget>()
                ?? base.FindGenericDeclarationsForType(provider);
        }
    }
}