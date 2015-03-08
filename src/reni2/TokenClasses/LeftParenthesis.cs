using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass, ITokenClassWithId
    {
        public static string Id(int level) => "\0{[(".Substring(level, 1);

        public LeftParenthesis(int level) { Level = level; }

        int Level { get; }

        string ITokenClassWithId.Id => Id(Level);

        protected override ReniParser.Syntax Suffix(ReniParser.Syntax left, Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token)
                .SurroundCompileSyntax(left);

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, Token token, ReniParser.Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token)
                .SurroundCompileSyntax(left, right);

        protected override ReniParser.Syntax Prefix(Token token, ReniParser.Syntax right)
            => right.Surround(new Syntax(token));

        protected override ReniParser.Syntax Terminal(Token token)
            => new Syntax(token);

        sealed class Syntax : ReniParser.Syntax
        {
            public Syntax(Token token)
                : base(token) {}
            Syntax(Syntax other, ParsedSyntax[] parts)
                : base(other, parts) {}

            internal override CompileSyntax ToCompiledSyntax
                => new EmptyList(Token).SurroundCompileSyntax(this);

            internal override ReniParser.Syntax Surround(params ParsedSyntax[] parts)
                => new Syntax(this, parts);
        }
    }
}