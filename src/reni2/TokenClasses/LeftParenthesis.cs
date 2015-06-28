using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Parser;
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

        protected override Checked<Parser.Syntax> Suffix
            (Parser.Syntax left, SourcePart token)
            => new Syntax(Level, token, null)
                .Issues(IssueId.UnexpectedUseAsSuffix.CreateIssue(token));

        protected override Checked<Parser.Syntax> Infix
            (Parser.Syntax left, SourcePart token, Parser.Syntax right)
            => new Syntax(Level, token, right)
                .Issues(IssueId.UnexpectedUseAsInfix.CreateIssue(token));

        protected override Checked<Parser.Syntax> Prefix
            (SourcePart token, Parser.Syntax right)
            => new Syntax(Level, token, right);

        protected override Checked<Parser.Syntax> Terminal(SourcePart token)
            => new Syntax(Level, token, null);

        sealed class Syntax : Parser.Syntax
        {
            SourcePart Token { get; }
            readonly int Level;

            [EnableDump]
            Parser.Syntax Right { get; }

            public Syntax(int level, SourcePart token, Parser.Syntax right)
            {
                Token = token;
                Level = level;
                Right = right;
            }

            [DisableDump]
            protected override IEnumerable<Parser.Syntax> DirectChildren
            {
                get { yield return Right; }
            }

            internal override Checked<ExclamationSyntaxList> ExclamationSyntax(SourcePart token)
                => new Checked<ExclamationSyntaxList>
                    (
                    ExclamationSyntaxList.Create(token),
                    IssueId.UnexpectedDeclarationTag.CreateIssue(Token)
                    );


            [DisableDump]
            internal override Checked<CompileSyntax> ToCompiledSyntax
            {
                get
                {
                    var right = (Right ?? new EmptyList(Token)).ToCompiledSyntax;
                    return new Checked<CompileSyntax>
                        (
                        right.Value,
                        IssueId.MissingRightBracket.CreateIssue(Token).plus(right.Issues)
                        );
                }
            }

            internal override Checked<Parser.Syntax> Match(int level, SourcePart token)
            {
                Tracer.Assert(Level == level);
                return (Right ?? new EmptyList(Token));
            }
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as RightParenthesis)?.Level == Level;
    }
}