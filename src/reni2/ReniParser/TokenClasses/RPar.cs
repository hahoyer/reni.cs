using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class RPar : TokenClass
    {
        private readonly int _level;

        internal RPar(int level)
        {
            _level = level;
            Name = "$}])";
        }

        protected override bool IsEnd { get { return _level == 0; } }
        protected override string PrioTableName(string name) { return _level == 0 ? "<end>" : base.PrioTableName(name); }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(right == null);
            if(left != null)
                return left.RightParenthesis(_level, token);
            return base.Syntax(left, token, right);
        }

        internal int Level { get { return _level; } }
    }
}