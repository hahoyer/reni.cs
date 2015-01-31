using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class LeftParenthesis : TokenClass
    {
        readonly int _level;

        internal LeftParenthesis(int level) { _level = level; }

        [DisableDump]
        internal int Level { get { return _level; } }

        protected override Syntax Prefix(SourcePart token, Syntax right)
        {
            return new LeftParenthesisSyntax(_level, token, right);
        }

        protected override Syntax Terminal(SourcePart token)
        {
            return new LeftParenthesisSyntax(_level, token, null);
        }
    }
}