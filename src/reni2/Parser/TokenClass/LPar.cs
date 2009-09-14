using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Syntax;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Left parenthesis' 
    /// </summary>
    [Serializable]
    internal sealed class LPar : TokenClassBase
    {
        private readonly int _level;

        private static readonly TokenClassBase _parenthesis = new LPar(3);
        private static readonly TokenClassBase _bracket = new LPar(2);
        private static readonly TokenClassBase _brace = new LPar(1);
        private static readonly TokenClassBase _frame = new LPar(0);

        internal static TokenClassBase Parenthesis { get { return _parenthesis; } }
        internal static TokenClassBase Bracket { get { return _bracket; } }
        internal static TokenClassBase Brace { get { return _brace; } }
        internal static TokenClassBase Frame { get { return _frame; } }


        private LPar(int level)
        {
            _level = level;
            Name = "<left " + _level + ">";
        }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            ParsedSyntaxExtender.AssertIsNull(left);
            if(right == null)
                return new EmptyList(token);
            return right.SurroundedByParenthesis(token);
        }

        internal override string PrioTableName(string name)
        {
            if(_level == 0)
                return "<frame>";

            return base.PrioTableName(name);
        }
    }
}