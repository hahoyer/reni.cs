using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class RightParenthesis : TokenClass
    {
        readonly int _level;

        internal RightParenthesis(int level) { _level = level; }

        protected override Syntax Suffix(Syntax left, SourcePart token) { return left.RightParenthesis(_level, token); }
    }

    sealed class EndToken : TokenClass
    {
        protected override Syntax Suffix(Syntax left, SourcePart token) { return left; }
    }
}