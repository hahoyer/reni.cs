using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class LeftParenthesis : TokenClass
    {
        private readonly int _level;

        internal LeftParenthesis(int level)
        {
            _level = level;
        }

        [IsDumpEnabled(false)]
        internal int Level { get { return _level; } }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            Tracer.Assert(left == null);
            return new Syntax.LeftParenthesis(_level, token, right);
        }
    }
}