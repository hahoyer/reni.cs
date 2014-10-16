using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class LeftParenthesis : TokenClass, IInfix, ITerminal
    {
        readonly int _level;

        internal LeftParenthesis(int level) { _level = level; }

        [DisableDump]
        internal int Level { get { return _level; } }

        protected override ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right)
        {
            return new Syntax.LeftParenthesis(_level, null, this, token, right);
        }
        protected override ParsedSyntax TerminalSyntax(TokenData token) { return new EmptyList.Half(token); }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return context.FunctionalObjectResult(category, left, right);
        }
        Result ITerminal.Result(ContextBase context, Category category, TokenData token)
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