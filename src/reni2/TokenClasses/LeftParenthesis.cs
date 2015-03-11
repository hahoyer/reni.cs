using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
        public static string Id(int level) => "\0([{".Substring(level, 1);

        public LeftParenthesis(int level) { Level = level; }

        int Level { get; }

        string ITokenClassWithId.Id => Id(Level);

        protected override ReniParser.Syntax Suffix(ReniParser.Syntax left, Token token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, Token token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override ReniParser.Syntax Prefix(Token token, ReniParser.Syntax right)
            => new Syntax(Level, token, right);

        protected override ReniParser.Syntax Terminal(Token token)
            => new Syntax(Level, token, null);

        sealed class Syntax : ReniParser.Syntax
        {
            readonly int _level;
            internal readonly ReniParser.Syntax Right;

            public Syntax(int level, Token token, ReniParser.Syntax right)
                : base(token)
            {
                _level = level;
                Right = right;
            }

            Syntax(Syntax other, ParsedSyntax[] parts)
                : base(other, parts) { Right = other.Right; }

            internal override CompileSyntax ToCompiledSyntax
            => new CompileSyntaxError(IssueId.MissingRightBracket, Token)
                .SurroundCompileSyntax(Right);

            internal override bool IsBraceLike => true;

            internal override ReniParser.Syntax RightParenthesis
                (RightParenthesis.Syntax rightBracket)
            {
                Tracer.Assert(_level == rightBracket.Level);
                return (Right?? new EmptyList(Token.SourcePart.End))
                    .Surround(this, rightBracket);
            }
        }
    }
}