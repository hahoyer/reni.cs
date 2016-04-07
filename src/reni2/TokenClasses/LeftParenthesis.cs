﻿using System;
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

        protected override Checked<Parser.Syntax> Suffix
            (Parser.Syntax left, SourcePart token)
            => new Syntax(left, Level, token, null);

        protected override Checked<Parser.Syntax> Infix
            (Parser.Syntax left, SourcePart token, Parser.Syntax right)
            => new Syntax(left, Level, token, right);

        protected override Checked<Parser.Syntax> Prefix
            (SourcePart token, Parser.Syntax right)
            => new Syntax(null, Level, token, right);

        protected override Checked<Parser.Syntax> OldTerminal(SourcePart token)
            => new Syntax(null, Level, token, null);

        sealed class Syntax : Parser.Syntax
        {
            internal override SourcePart Token { get; }
            readonly int Level;

            [EnableDump]
            Parser.Syntax Right { get; }
            [EnableDump]
            Parser.Syntax Left { get; }

            public Syntax(Parser.Syntax left, int level, SourcePart token, Parser.Syntax right)
            {
                Left = left;
                Token = token;
                Level = level;
                Right = right;
            }

            [DisableDump]
            protected override IEnumerable<Parser.Syntax> DirectChildren
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
            internal override Checked<CompileSyntax> ToCompiledSyntax
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

            internal override Checked<Parser.Syntax> Match(int level, SourcePart token)
            {
                if(Level != level)
                    return new Checked<Parser.Syntax>
                        (this, IssueId.ExtraLeftBracket.CreateIssue(Token));

                var innerPart = Right ?? new EmptyList(token);

                if(Level == 0)
                {
                    Tracer.Assert(Left == null);
                    return Checked<Parser.Syntax>.From(innerPart.ToCompiledSyntax);
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