using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.TokenClasses
{
    [Serializable]
    internal sealed class RightParenthesis : TokenClass
    {
        private readonly int _level;

        internal RightParenthesis(int level)
        {
            _level = level;
        }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            if(left != null)
                return left.RightParenthesis(_level, token);
            return base.Syntax(left, token, right);
        }
    }
}