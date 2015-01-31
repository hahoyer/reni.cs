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
        [EnableDump]
        readonly int _parenthesis;
        [EnableDump]
        readonly Syntax _right;

        public LeftParenthesisSyntax
            (int parenthesis, SourcePart token, Syntax right)
            : base(token)
        {
            _parenthesis = parenthesis;
            _right = right;
        }

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            Tracer.Assert(level == _parenthesis);
            if(_right == null)
                return new EmptyList(token);
            return _right;
        }
    }

    sealed class RightParenthesisSyntax : Syntax
    {
        [EnableDump]
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