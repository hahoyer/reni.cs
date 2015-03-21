using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
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
    sealed class LeftParenthesis : TokenClass
    {
        public static string TokenId(int level) => "\0([{".Substring(level, 1);

        public LeftParenthesis(int level) { Level = level; }

        int Level { get; }

        public override string Id => TokenId(Level);

        protected override ReniParser.Syntax Suffix(ReniParser.Syntax left, IToken token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, IToken token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override ReniParser.Syntax Prefix(IToken token, ReniParser.Syntax right)
            => new Syntax(Level, token, right);

        protected override ReniParser.Syntax Terminal(IToken token)
            => new Syntax(Level, token, null);

        sealed class Syntax : ReniParser.Syntax
        {
            SourcePart Token { get; }
            readonly int _level;
            internal readonly ReniParser.Syntax Right;

            public Syntax(int level, IToken token, ReniParser.Syntax right)
            {
                Token = token.Characters;
                _level = level;
                Right = right;
            }

            protected override IEnumerable<ReniParser.Syntax> DirectChildren
            {
                get { yield return Right; }
            }

            [DisableDump]
            internal override CompileSyntax ToCompiledSyntax
                => new CompileSyntaxError(IssueId.MissingRightBracket, Token);

            internal override bool IsBraceLike => true;

            internal override ReniParser.Syntax Match(int level, SourcePart token)
            {
                Tracer.Assert(_level == level);
                return (Right ?? new EmptyList());
            }
        }
    }
}