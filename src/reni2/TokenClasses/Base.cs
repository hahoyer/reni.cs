using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass
    {
    }

    abstract class NonPrefixToken : TokenClass
    {
    }

    abstract class InfixPrefixToken : TokenClass
    {
    }

    abstract class NonSuffixToken : TokenClass
    {
    }

    abstract class SuffixToken : TokenClass
    {
    }

    abstract class InfixToken : TokenClass
    {
    }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal, IValueProvider
    {
        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            if(left == null && right == null)
                return new TerminalSyntax(token, this);

            NotImplementedMethod(left, token, right);
            return null;
        }

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        Value ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        protected Value Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueProvider
    {
        Result IInfix.Result
            (ContextBase context, Category category, Value left, Value right)
            => Result(context, category, left, right);

        Result IPrefix.Result
            (ContextBase context, Category category, PrefixSyntax token, Value right)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, Value left, Value right);

        protected abstract Result Result
            (ContextBase context, Category category, Value right);

        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            if(left != null && right != null)
                return InfixSyntax.Create(left.Value, this, token, right.Value);

            if (left == null && right != null)
                return PrefixSyntax.Create(this, token, right.Value);

            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix,IValueProvider
    {
        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        Result IPrefix.Result
            (ContextBase context, Category category, PrefixSyntax token, Value right)
            => Result(context, category, token, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, PrefixSyntax token, Value right);

        Value ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual Value Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            if (left == null)
            {
                if(right == null)
                    return new TerminalSyntax(token, this);

                return PrefixSyntax.Create(this, token, right.Value);
            }

            NotImplementedMethod(left, token, right);
            return null;
        }
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix, IValueProvider
    {
        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            if(right == null)
                return SuffixSyntax.Create(left.Value, this, token);

            NotImplementedMethod(left, token, right);
            return null;
        }

        Result ISuffix.Result(ContextBase context, Category category, Value left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, Value left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix, IValueProvider
    {
        Result IInfix.Result
            (ContextBase context, Category category, Value left, Value right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, Value left, Value right);

        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
            => InfixSyntax.Create(left.Value, this, token, right.Value);
    }
}