using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis
        : TokenClass
            , IBelongingsMatcher
            , IBracketMatch<Syntax>
    {
        internal sealed class Matched : TokenClass
        {
            protected override Checked<OldSyntax> OldTerminal(SourcePart token)
            {
                NotImplementedMethod(token);
                return null;
            }

            protected override Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            {
                NotImplementedMethod(token, right);
                return null;
            }

            protected override Checked<OldSyntax> OldSuffix(OldSyntax left, SourcePart token) => left;

            protected override Checked<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
                => left.InfixOfMatched(token, right);

            [DisableDump]
            internal override bool IsVisible => false;
            [DisableDump]
            public override string Id => "()";
        }

        public static string TokenId(int level)
            => level == 0 ? PrioTable.EndOfText : "\0}])".Substring(level, 1);

        public RightParenthesis(int level) { Level = level; }

        [DisableDump]
        internal int Level { get; }
        [DisableDump]
        public override string Id => TokenId(Level);
        [DisableDump]
        internal override bool IsVisible => Level != 0;

        protected override Checked<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => left.Match(Level, token);

        protected override Checked<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override Checked<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected override Checked<OldSyntax> OldTerminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        IType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
    }
}