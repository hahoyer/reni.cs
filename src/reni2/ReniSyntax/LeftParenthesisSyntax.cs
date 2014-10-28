using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    sealed class LeftParenthesisSyntax : Syntax
    {
        readonly int _leftLevel;
        [EnableDump]
        readonly Syntax _left;
        [EnableDump]
        readonly LeftParenthesis _parenthesis;
        [EnableDump]
        readonly Syntax _right;

        public LeftParenthesisSyntax
            (int leftLevel, Syntax left, LeftParenthesis parenthesis, SourcePart token, Syntax right)
            : base(token)
        {
            _leftLevel = leftLevel;
            _left = left;
            _parenthesis = parenthesis;
            _right = right;
        }

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            if(level != _leftLevel)
                throw new ParenthesisMissmatchException(this, level, token);
            var surroundedByParenthesis = SurroundedByParenthesis(token);
            if(_left == null)
                return surroundedByParenthesis;
            Tracer.Assert(_right != null, () => Dump() + "\ntoken=" + token.Dump());
            return new InfixSyntax(token, _left.ToCompiledSyntax(), _parenthesis, _right.ToCompiledSyntax());
        }

        Syntax SurroundedByParenthesis(SourcePart token)
        {
            return _right == null ? new EmptyList(Token) : _right.SurroundedByParenthesis(Token, token);
        }

        sealed class ParenthesisMissmatchException : Exception
        {
            readonly LeftParenthesisSyntax _leftParenthesis;
            readonly int _level;
            readonly SourcePart _token;

            public ParenthesisMissmatchException(LeftParenthesisSyntax leftParenthesis, int level, SourcePart token)
            {
                _leftParenthesis = leftParenthesis;
                _level = level;
                _token = token;
            }
        }
    }

    sealed class RightParenthesisSyntax : Syntax
    {
        readonly int _rightLevel;
        [EnableDump]
        readonly Syntax _left;
        [EnableDump]
        readonly RightParenthesis _parenthesis;
        [EnableDump]
        readonly Syntax _right;

        public RightParenthesisSyntax
            (int level, Syntax left, RightParenthesis parenthesis, SourcePart token, Syntax right)
            : base(token)
        {
            _rightLevel = level;
            _left = left;
            _parenthesis = parenthesis;
            _right = right;
        }
    }
}