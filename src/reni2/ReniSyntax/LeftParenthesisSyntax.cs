using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    sealed class LeftParenthesisPrefixSyntax : Syntax
    {
        [EnableDump]
        readonly int _level;
        [EnableDump]
        readonly Syntax _right;

        public LeftParenthesisPrefixSyntax
            (int level, SourcePart token, Syntax right)
            : base(token)
        {
            _level = level;
            _right = right;
        }

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            Tracer.Assert(level == _level);
            return _right;
        }
    }

    sealed class LeftParenthesisSyntax : Syntax
    {
        [EnableDump]
        readonly Syntax _left;
        [EnableDump]
        readonly int _parenthesis;
        [EnableDump]
        readonly Syntax _right;
        static readonly IInfix _operator = new FunctionCallOperator();

        sealed class FunctionCallOperator : DumpableObject, IInfix
        {
            Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            {
                NotImplementedMethod(context, category, left, right);
                return null;
            }
        }

        public LeftParenthesisSyntax
            (Syntax left, int parenthesis, SourcePart token, Syntax right)
            : base(token)
        {
            _left = left;
            _parenthesis = parenthesis;
            _right = right;
        }

        internal override Syntax RightParenthesisOnLeft(int level, SourcePart token)
        {
            if(level != _parenthesis)
                throw new ParenthesisMissmatchException(this, level, token);
            var surroundedByParenthesis = SurroundedByParenthesis(token);
            if(_left == null)
                return surroundedByParenthesis;
            Tracer.Assert(_right != null, () => Dump() + "\ntoken=" + token.Dump());
            return new InfixSyntax(token, _left.ToCompiledSyntax(), _operator, _right.ToCompiledSyntax());
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