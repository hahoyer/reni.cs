using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass {}

    abstract class InfixPrefixToken : TokenClass {}

    abstract class NonSuffixToken : TokenClass {}

    abstract class SuffixToken : TokenClass {}

    abstract class InfixToken : TokenClass {}

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal, IValueProvider, SyntaxFactory.IValueToken
    {
        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        protected ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Terminal;

    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueProvider, SyntaxFactory.IValueToken
    {
        Result IInfix.Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result
            (ContextBase context, Category category, ValueSyntax right, BinaryTree token)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right);

        protected abstract Result Result
            (ContextBase context, Category category, ValueSyntax right);

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.InfixPrefix;
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix, IValueProvider, SyntaxFactory.IValueToken
    {
        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category);

        protected abstract Result Result
            (ContextBase context, Category category);

        Result IPrefix.Result
            (ContextBase context, Category category, ValueSyntax right, BinaryTree token)
            => Result(context, category, right, token);

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax right, BinaryTree token);

        ValueSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.NonSuffix;
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix, IValueProvider, SyntaxFactory.IValueToken
    {
        Result ISuffix.Result(ContextBase context, Category category, ValueSyntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, ValueSyntax left);

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Suffix;
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix, IValueProvider, SyntaxFactory.IValueToken
    {
        Result IInfix.Result
            (ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, ValueSyntax left, ValueSyntax right);

        SyntaxFactory.IValueProvider SyntaxFactory.IValueToken.Provider => SyntaxFactory.Infix;
    }
}