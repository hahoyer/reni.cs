using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal sealed class LPar : TokenClass
    {
        private readonly int _level;

        internal LPar(int level)
        {
            _level = level;
            Name = "^{[(";
        }

        [IsDumpEnabled(false)]
        internal int Level { get { return _level; } }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            Tracer.Assert(left == null);
            return new LeftParSyntax(_level, token, right);
        }

        protected override string PrioTableName(string name) { return _level == 0 ? "<frame>" : base.PrioTableName(name); }
    }
}