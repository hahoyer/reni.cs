﻿using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass { }

    abstract class InfixPrefixToken : TokenClass { }

    abstract class NonSuffixToken : TokenClass { }

    abstract class SuffixToken : TokenClass { }

    abstract class InfixToken : TokenClass { }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal, IValueToken
    {
        Result ITerminal.Result(ContextBase context, Category category, IToken token)
            => Result(context, category, token);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        IValueProvider IValueToken.Provider => Factory.Terminal;

        protected abstract Result Result(ContextBase context, Category category, IToken token);

        protected ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueToken
    {
        Result IInfix.Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result(ContextBase context, Category category, ValueSyntax right, IToken token)
            => Result(context, category, right);

        IValueProvider IValueToken.Provider => Factory.InfixPrefix;

        protected abstract Result Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right);

        protected abstract Result Result(ContextBase context, Category category, ValueSyntax right);
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix, IValueToken
    {
        Result IPrefix.Result(ContextBase context, Category category, ValueSyntax right, IToken token)
            => Result(context, category, right, token);

        Result ITerminal.Result(ContextBase context, Category category, IToken token)
            => Result(context, category);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        IValueProvider IValueToken.Provider => Factory.NonSuffix;

        protected abstract Result Result(ContextBase context, Category category);

        protected abstract Result Result(ContextBase callContext, Category category, ValueSyntax right, IToken token);

        internal virtual ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix, IValueToken
    {
        Result ISuffix.Result(ContextBase context, Category category, ValueSyntax left)
            => Result(context, category, left);

        IValueProvider IValueToken.Provider => Factory.Suffix;

        protected abstract Result Result(ContextBase context, Category category, ValueSyntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix, IValueToken
    {
        Result IInfix.Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        IValueProvider IValueToken.Provider => Factory.Infix;

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax left, ValueSyntax right);
    }
}