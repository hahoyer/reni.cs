using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
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

        [IsDumpEnabled(false)]
        internal int Level { get { return _level; } }

        internal override IParsedSyntax CreateSyntax(IParsedSyntax left, Token token, IParsedSyntax right)
        {
            left.AssertIsNull();
            return new LeftParSyntax(token, right);
        }

        internal override string PrioTableName(string name)
        {
            if(_level == 0)
                return "<frame>";

            return base.PrioTableName(name);
        }
    }

    internal class LeftParSyntax : ParsedSyntax
    {
        [IsDumpEnabled(true)]
        private readonly IParsedSyntax _right;

        public LeftParSyntax(Token token, IParsedSyntax right)
            : base(token)
        {
            _right = right;
        }

        protected override IParsedSyntax RightPar(Token token) {
            if(((RPar) token.TokenClass).Level != ((LPar) Token.TokenClass).Level)
                return base.RightPar(token);
            if (_right == null)
                return new EmptyList(Token,token);
            return _right.SurroundedByParenthesis(Token,token);
        }
    }
}