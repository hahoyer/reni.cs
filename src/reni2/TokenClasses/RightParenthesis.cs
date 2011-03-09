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
            Name = "$}])";
        }

        protected override bool IsEnd { get { return _level == 0; } }
        protected override string PrioTableName(string name) { return _level == 0 ? "<end>" : base.PrioTableName(name); }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            if(left != null)
                return left.RightParenthesis(_level, token);
            return base.Syntax(left, token, right);
        }

        internal int Level { get { return _level; } }
    }
}