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
                return context.FunctionalObjectResult(category, left, right);
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

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            Tracer.Assert(level == _parenthesis);
            if(_left == null && _right == null)
                return new EmptyList(token);
            if(_left != null && _right != null)
                return new InfixSyntax(token, _left.ToCompiledSyntax, _operator, _right.ToCompiledSyntax);
            return base.RightParenthesis(level, token);
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