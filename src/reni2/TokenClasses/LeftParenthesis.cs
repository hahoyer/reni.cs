using System;
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
    sealed class LeftParenthesis : TokenClass, IInfix, ITerminal
    {
        readonly int _level;

        internal LeftParenthesis(int level) { _level = level; }

        [DisableDump]
        internal int Level { get { return _level; } }

        protected override Syntax PrefixSyntax(SourcePart token, Syntax right)
        {
            return new ReniSyntax.LeftParenthesis(_level, null, this, token, right);
        }
        protected override Syntax InfixSyntax(Syntax left, SourcePart token, Syntax right)
        {
            return new ReniSyntax.LeftParenthesis(_level, left, this, token, right);
        }

        protected override Syntax TerminalSyntax(SourcePart token) { return new EmptyList.Half(token); }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return context.FunctionalObjectResult(category, left, right);
        }
        Result ITerminal.Result(ContextBase context, Category category, SourcePart token)
        {
            NotImplementedMethod(context, category, token);
            return null;
        }
        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }
}