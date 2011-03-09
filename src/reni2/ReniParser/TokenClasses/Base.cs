using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.ReniParser.TokenClasses
{
    [Serializable]
    internal abstract class Special : TokenClass
    {}

    [Serializable]
    internal abstract class Terminal : Special, ITerminal
    {
        protected override sealed ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new TerminalSyntax(token, this);
        }

        public abstract Result Result(ContextBase context, Category category, TokenData token);
    }

    [Serializable]
    internal abstract class Prefix : Special, IPrefix
    {
        public abstract Result Result(ContextBase context, Category category, ICompileSyntax right);

        protected override sealed ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            left.AssertIsNull();
            return new PrefixSyntax(token, this, right.CheckedToCompiledSyntax());
        }
    }

    [Serializable]
    internal abstract class Suffix : Special, ISuffix
    {
        public abstract Result Result(ContextBase context, Category category, ICompileSyntax right);

        protected override sealed ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            right.AssertIsNull();
            return new SuffixSyntax(token, left.CheckedToCompiledSyntax(), this);
        }
    }

    [Serializable]
    internal abstract class Infix : Special, IInfix
    {
        public abstract Result Result(ContextBase callContext, Category category, ICompileSyntax left, ICompileSyntax right);

        protected override sealed ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right) { return new InfixSyntax(token, left.CheckedToCompiledSyntax(), this, right.CheckedToCompiledSyntax()); }
    }
}