using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class RightParenthesis : TokenClass
    {
        readonly int _level;

        internal RightParenthesis(int level) { _level = level; }

        protected override ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token) { return left.RightParenthesis(_level, token); }
        protected override bool IsEnd { get { return _level == 0; } }
        protected override bool AcceptsMatch { get { return true; } }
    }
}