using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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

        protected override ReniParser.Syntax Suffix(ReniParser.Syntax left, SourcePart token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, SourcePart token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override ReniParser.Syntax Prefix(SourcePart token, ReniParser.Syntax right)
            => new Syntax(Level, token, right);

        protected override ReniParser.Syntax Terminal(SourcePart token)
            => new Syntax(Level, token, null);

        sealed class Syntax : ReniParser.Syntax
        {
            SourcePart Token { get; }
            readonly int _level;

            [EnableDump]
            ReniParser.Syntax Right { get; }

            public Syntax(int level, SourcePart token, ReniParser.Syntax right)
            {
                Token = token;
                _level = level;
                Right = right;
            }

            [DisableDump]
            protected override IEnumerable<ReniParser.Syntax> DirectChildren
            {
                get { yield return Right; }
            }

            [DisableDump]
            internal override CompileSyntax ToCompiledSyntax
                => IssueId.MissingRightBracket.Syntax(Token, Right);

            internal override bool IsBraceLike => true;
            internal override bool IsKeyword => true;

            internal override ReniParser.Syntax Match(int level, SourcePart token)
            {
                Tracer.Assert(_level == level);
                return (Right ?? new EmptyList());
            }
        }
    }
}