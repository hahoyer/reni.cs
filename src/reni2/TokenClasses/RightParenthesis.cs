using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis : TokenClass, ITokenClassWithId
    {
        public static string Id(int level) => "\0}])".Substring(level, 1);
        readonly int _level;

        public RightParenthesis(int level) { _level = level; }

        string ITokenClassWithId.Id => Id(_level);
        protected override Syntax Suffix(Syntax left, Token token)
            => left.RightParenthesis(_level, token);
        protected override Syntax Infix(Syntax left, Token token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
        protected override Syntax Terminal(Token token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }
}