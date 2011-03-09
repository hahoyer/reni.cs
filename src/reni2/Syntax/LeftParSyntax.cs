using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Syntax
{
    internal sealed class LeftParSyntax : ReniParser.ParsedSyntax
    {
        private readonly int _leftLevel;

        [IsDumpEnabled(true)]
        private readonly ReniParser.ParsedSyntax _right;

        public LeftParSyntax(int leftLevel, TokenData token, ReniParser.ParsedSyntax right)
            : base(token)
        {
            _leftLevel = leftLevel;
            _right = right;
        }

        protected override TokenData GetLastToken() { return _right.LastToken; }

        internal override ReniParser.ParsedSyntax RightParenthesis(int level, TokenData token)
        {
            if(level != _leftLevel)
                throw new ParenthesisMissmatchException(this, level, token);
            if(_right == null)
                return new EmptyList(Token, token);
            return _right.SurroundedByParenthesis(Token, token);
        }

        private sealed class ParenthesisMissmatchException : Exception
        {
            private readonly LeftParSyntax _leftParSyntax;
            private readonly int _level;
            private readonly TokenData _token;

            public ParenthesisMissmatchException(LeftParSyntax leftParSyntax, int level, TokenData token)
            {
                _leftParSyntax = leftParSyntax;
                _level = level;
                _token = token;
            }
        }
    }
}