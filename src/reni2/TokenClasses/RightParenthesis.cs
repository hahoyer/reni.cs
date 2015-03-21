using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis : TokenClass
    {
        public static string TokenId(int level) => "\0)]}".Substring(level, 1);
        readonly int _level;

        public RightParenthesis(int level) { _level = level; }

        public override string Id => TokenId(_level);

        protected override Syntax Suffix(Syntax left, SourcePart token)
            => left.Match(_level, token);

        protected override Syntax Infix
            (Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override Syntax Prefix(SourcePart token, Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected override Syntax Terminal(SourcePart token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }
}