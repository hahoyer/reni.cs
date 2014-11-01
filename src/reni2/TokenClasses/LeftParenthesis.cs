﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class LeftParenthesis : TokenClass, IInfix
    {
        readonly int _level;

        internal LeftParenthesis(int level) { _level = level; }

        [DisableDump]
        internal int Level { get { return _level; } }

        protected override Syntax Prefix(SourcePart token, Syntax right)
        {
            return new LeftParenthesisPrefixSyntax(_level, token, right);
        }

        protected Syntax InfixSyntax(Syntax left, SourcePart token, Syntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
            return new LeftParenthesisSyntax(left, _level, token, right);
        }

        protected Syntax Terminal(SourcePart token)
        {
            return new LeftParenthesisSyntax(null, _level, token, null);
        }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return context.FunctionalObjectResult(category, left, right);
        }
        Result ITerminal_Result(ContextBase context, Category category, SourcePart token)
        {
            NotImplementedMethod(context, category, token);
            return null;
        }
        CompileSyntax ITerminal_Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }
}