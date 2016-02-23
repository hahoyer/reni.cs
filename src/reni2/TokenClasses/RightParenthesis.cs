﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis
        : TokenClass
            , IBelongingsMatcher
            , IBracketMatch<SourceSyntax>
    {
        internal sealed class Matched : TokenClass
        {
            protected override Checked<Syntax> Terminal(SourcePart token)
            {
                NotImplementedMethod(token);
                return null;
            }

            protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            {
                NotImplementedMethod(token, right);
                return null;
            }

            protected override Checked<Syntax> Suffix(Syntax left, SourcePart token) => left;

            protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
                => left.InfixOfMatched(token, right);

            internal override bool IsVisible => false;

            public override string Id => "()";
        }

        public static string TokenId(int level)
            => level == 0 ? PrioTable.EndOfText : "\0}])".Substring(level, 1);

        public RightParenthesis(int level) { Level = level; }

        [DisableDump]
        internal int Level { get; }
        public override string Id => TokenId(Level);
        internal override bool IsVisible => Level != 0;

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => left.Match(Level, token);

        protected override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override Checked<Syntax> Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected override Checked<Syntax> Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        IType<SourceSyntax> IBracketMatch<SourceSyntax>.Value { get; } = new Matched();
    }
}