using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.Syntax
{
    sealed class LeftParenthesis : ParsedSyntax
    {
        readonly int _leftLevel;
        [EnableDump]
        readonly ParsedSyntax _left;
        [EnableDump]
        readonly TokenClasses.LeftParenthesis _parenthesis;
        [EnableDump]
        readonly ParsedSyntax _right;

        public LeftParenthesis
            (int leftLevel, ParsedSyntax left, TokenClasses.LeftParenthesis parenthesis, TokenData token, ParsedSyntax right)
            : base(token)
        {
            _leftLevel = leftLevel;
            _left = left;
            _parenthesis = parenthesis;
            _right = right;
        }

        [DisableDump]
        internal override TokenData FirstToken
        {
            get
            {
                if(_left != null)
                    return _left.LastToken;
                return base.FirstToken;
            }
        }

        [DisableDump]
        internal override TokenData LastToken
        {
            get
            {
                if(_right != null)
                    return _right.LastToken;
                return base.LastToken;
            }
        }

        internal override ParsedSyntax RightParenthesis(int level, TokenData token)
        {
            if(level != _leftLevel)
                throw new ParenthesisMissmatchException(this, level, token);
            var surroundedByParenthesis = SurroundedByParenthesis(token);
            if(_left == null)
                return surroundedByParenthesis;
            Tracer.Assert(_right != null, () => Dump() + "\ntoken=" + token.Dump());
            return new InfixSyntax(token, _left.ToCompiledSyntax(), _parenthesis, _right.ToCompiledSyntax());
        }

        ParsedSyntax SurroundedByParenthesis(TokenData token)
        {
            return _right == null ? new EmptyList(Token, token) : _right.SurroundedByParenthesis(Token, token);
        }

        sealed class ParenthesisMissmatchException : Exception
        {
            readonly LeftParenthesis _leftParenthesis;
            readonly int _level;
            readonly TokenData _token;

            public ParenthesisMissmatchException(LeftParenthesis leftParenthesis, int level, TokenData token)
            {
                _leftParenthesis = leftParenthesis;
                _level = level;
                _token = token;
            }
        }
    }
}