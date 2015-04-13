using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Formatting;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level) => "\0([{".Substring(level, 1);

        public LeftParenthesis(int level) { Level = level; }
        [DisableDump]
        internal int Level { get; }

        public override string Id => TokenId(Level);

        protected override Checked<ReniParser.Syntax> Suffix
            (ReniParser.Syntax left, SourcePart token)
            => new Syntax(Level, token, null)
                .Issues(IssueId.UnexpectedUseAsSuffix.CreateIssue(token));

        protected override Checked<ReniParser.Syntax> Infix
            (ReniParser.Syntax left, SourcePart token, ReniParser.Syntax right)
            => new Syntax(Level, token, right)
                .Issues(IssueId.UnexpectedUseAsInfix.CreateIssue(token));

        protected override Checked<ReniParser.Syntax> Prefix
            (SourcePart token, ReniParser.Syntax right)
            => new Syntax(Level, token, right);

        protected override Checked<ReniParser.Syntax> Terminal(SourcePart token)
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
            internal override Checked<CompileSyntax> ToCompiledSyntax
            {
                get
                {
                    var right = (Right ?? new EmptyList()).ToCompiledSyntax;
                    return new Checked<CompileSyntax>
                        (
                        right.Value,
                        IssueId.MissingRightBracket.CreateIssue(Token).plus(right.Issues)
                        );
                }
            }

            internal override Checked<ReniParser.Syntax> Match(int level, SourcePart token)
            {
                Tracer.Assert(_level == level);
                return (Right ?? new EmptyList());
            }
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as RightParenthesis)?.Level == Level;
    }
}