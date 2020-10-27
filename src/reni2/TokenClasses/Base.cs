using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass {}

    abstract class InfixPrefixToken : TokenClass {}

    abstract class NonSuffixToken : TokenClass {}

    abstract class SuffixToken : TokenClass {}

    abstract class InfixToken : TokenClass {}

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal, IValueToken
    {
        Result ITerminal.Result(ContextBase context, Category category, SourcePart token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, SourcePart token);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        protected ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        IValueProvider IValueToken.Provider => Factory.Terminal;

    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueToken
    {
        Result IInfix.Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result
            (ContextBase context, Category category, ValueSyntax right, SourcePart token)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right);

        protected abstract Result Result
            (ContextBase context, Category category, ValueSyntax right);

        IValueProvider IValueToken.Provider => Factory.InfixPrefix;
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix, IValueToken
    {
        Result ITerminal.Result(ContextBase context, Category category, SourcePart token)
            => Result(context, category);

        protected abstract Result Result
            (ContextBase context, Category category);

        Result IPrefix.Result
            (ContextBase context, Category category, ValueSyntax right, SourcePart token)
            => Result(context, category, right, token);

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax right, SourcePart token);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        IValueProvider IValueToken.Provider => Factory.NonSuffix;
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix, IValueToken
    {
        Result ISuffix.Result(ContextBase context, Category category, ValueSyntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, ValueSyntax left);

        IValueProvider IValueToken.Provider => Factory.Suffix;
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix, IValueToken
    {
        Result IInfix.Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax left, ValueSyntax right);

        IValueProvider IValueToken.Provider => Factory.Infix;
    }
}