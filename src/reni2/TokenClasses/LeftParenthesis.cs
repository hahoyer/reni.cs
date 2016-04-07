using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class LeftParenthesis : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level)
            => level == 0 ? PrioTable.BeginOfText : "\0{[(".Substring(level, 1);

        public LeftParenthesis(int level) { Level = level; }
        [DisableDump]
        internal int Level { get; }

        public override string Id => TokenId(Level);
        internal override bool IsVisible => Level != 0;

        protected override Checked<Parser.OldSyntax> Suffix
            (Parser.OldSyntax left, SourcePart token)
            => new Syntax(left, Level, token, null);

        protected override Checked<Parser.OldSyntax> Infix
            (Parser.OldSyntax left, SourcePart token, Parser.OldSyntax right)
            => new Syntax(left, Level, token, right);

        protected override Checked<Parser.OldSyntax> Prefix
            (SourcePart token, Parser.OldSyntax right)
            => new Syntax(null, Level, token, right);

        protected override Checked<Parser.OldSyntax> OldTerminal(SourcePart token)
            => new Syntax(null, Level, token, null);

        sealed class Syntax : Parser.OldSyntax
        {
            internal override SourcePart Token { get; }
            readonly int Level;

            [EnableDump]
            Parser.OldSyntax Right { get; }
            [EnableDump]
            Parser.OldSyntax Left { get; }

            public Syntax(Parser.OldSyntax left, int level, SourcePart token, Parser.OldSyntax right)
            {
                Left = left;
                Token = token;
                Level = level;
                Right = right;
            }

            [DisableDump]
            protected override IEnumerable<Parser.OldSyntax> DirectChildren
            {
                get
                {
                    yield return Left;
                    yield return Right;
                }
            }

            internal override Checked<ExclamationSyntaxList> ExclamationSyntax(SourcePart token)
                => new Checked<ExclamationSyntaxList>
                    (
                    ExclamationSyntaxList.Create(token),
                    IssueId.UnexpectedDeclarationTag.CreateIssue(Token)
                    );


            [DisableDump]
            internal override Checked<Value> ToCompiledSyntax
            {
                get
                {
                    if(Left == null)
                        return (Right ?? new EmptyList(Token)).ToCompiledSyntax;

                    if(Right != null)
                        NotImplementedFunction();

                    return Left.ToCompiledSyntax;
                }
            }

            internal override Checked<Parser.OldSyntax> Match(int level, SourcePart token)
            {
                if(Level != level)
                    return new Checked<Parser.OldSyntax>
                        (this, IssueId.ExtraLeftBracket.CreateIssue(Token));

                var innerPart = Right ?? new EmptyList(token);

                if(Level == 0)
                {
                    Tracer.Assert(Left == null);
                    return Checked<Parser.OldSyntax>.From(innerPart.ToCompiledSyntax);
                }

                if(Left == null)
                    return innerPart;

                NotImplementedMethod(level, token);
                return null;
            }
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as RightParenthesis)?.Level == Level;
    }
}