using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level) => "\0)]}".Substring(level, 1);

        public RightParenthesis(int level) { Level = level; }

        [DisableDump]
        internal int Level { get; }
        public override string Id => TokenId(Level);

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => left.Match(Level, token);

        protected override Checked<Syntax> Infix
            (Syntax left, SourcePart token, Syntax right)
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
    }
}