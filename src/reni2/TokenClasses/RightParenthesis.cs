using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
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

        protected override ReniParser.Syntax Suffix(ReniParser.Syntax left, IToken token)
            => left.Match(_level, token.Characters);

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, IToken token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override ReniParser.Syntax Prefix(IToken token, ReniParser.Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected override ReniParser.Syntax Terminal(IToken token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }
}