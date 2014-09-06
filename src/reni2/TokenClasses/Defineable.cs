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
    abstract class Defineable : TokenClass, ISearchObject
    {
        protected override sealed ParsedSyntax TerminalSyntax(TokenData token) { return new DefinableTokenSyntax(this, token); }
        protected override sealed ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right) { return new ExpressionSyntax(this, null, token, (CompileSyntax) right); }
        protected override sealed ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token) { return left.CreateSyntaxOrDeclaration(this, token, null); }
        protected override sealed ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right) { return left.CreateSyntaxOrDeclaration(this, token, right); }

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }

        ISearchResult ISearchObject.GetFeatureGenericized(ISearchTarget target) { return GetFeatureGenericized(target); }

        internal virtual ISearchResult GetFeatureGenericized(ISearchTarget target)
        {
            NotImplementedMethod(target);
            return null;
        }

        internal virtual SearchResult Declarations<TType>(TType target)
            where TType : TypeBase
        {
            NotImplementedMethod(target);
            return null;
        }
    }
}