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

        protected override Syntax Prefix(SourcePart token, Syntax right) { return right.RightParenthesisOnRight(_level, token); }
        protected override Syntax Suffix(Syntax left, SourcePart token) { return left.RightParenthesisOnLeft(_level, token); }
    }
}