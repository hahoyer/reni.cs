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
    sealed class RightParenthesis : TokenClass, ITokenClassWithId
    {
        public static string Id(int level) => "\0)]}".Substring(level, 1);
        readonly int _level;

        public RightParenthesis(int level) { _level = level; }

        string ITokenClassWithId.Id => Id(_level);

        protected override ReniParser.Syntax Suffix(ReniParser.Syntax left, Token token)
            => left.RightParenthesis(new Syntax(_level, token));

        protected override ReniParser.Syntax Infix
            (ReniParser.Syntax left, Token token, ReniParser.Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected override ReniParser.Syntax Prefix(Token token, ReniParser.Syntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected override ReniParser.Syntax Terminal(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }

        internal sealed class Syntax : ReniParser.Syntax
        {
            internal readonly int Level;
            public Syntax(int level, Token token)
                : base(token) { Level = level; }

            internal override bool IsBraceLike => true;
        }
    }
}