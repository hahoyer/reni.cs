using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.DeclarationOptions;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;
using Reni.Type;

namespace Reni.TokenClasses;

abstract class TerminalToken : TokenClass;
abstract class InfixPrefixToken : TokenClass;
abstract class NonSuffixToken : TokenClass;
abstract class SuffixToken : TokenClass;
abstract class InfixToken : TokenClass;

abstract class TerminalSyntaxToken : TerminalToken, ITerminal, IValueToken
{
    Declaration[] ITerminal.Declarations => Declarations;

    Result ITerminal.GetResult(ContextBase context, Category category, SourcePart token)
        => GetResult(context, category, token);

    TypeBase? ITerminal.TryGetTypeBase(SourcePart token) => TryGetTypeBase(token);

    ValueSyntax? ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

    IValueProvider IValueToken.Provider => Factory.Infix;
    protected abstract Declaration[] Declarations { get; }

    protected abstract Result GetResult(ContextBase context, Category category, SourcePart token);
    protected abstract TypeBase? TryGetTypeBase(SourcePart token);

    protected ValueSyntax? Visit(ISyntaxVisitor visitor)
    {
        NotImplementedMethod(visitor);
        return null;
    }
}

abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix, IValueToken
{
    Result IInfix.GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
        => GetResult(context, category, left, right);

    Result IPrefix.GetResult(ContextBase context, Category category, ValueSyntax right, SourcePart token)
        => GetResult(context, category, right);

    IValueProvider IValueToken.Provider => Factory.Infix;

    protected abstract Result GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right);

    protected abstract Result GetResult(ContextBase context, Category category, ValueSyntax right);
}

abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix, IValueToken
{
    Result IPrefix.GetResult(ContextBase context, Category category, ValueSyntax right, SourcePart token)
        => GetResult(context, category, right, token);

    Declaration[] ITerminal.Declarations => Declarations;

    Result ITerminal.GetResult(ContextBase context, Category category, SourcePart token)
        => GetResult(context, category);

    TypeBase? ITerminal.TryGetTypeBase(SourcePart token)
    {
        NotImplementedMethod(token);
        return default;
    }

    ValueSyntax? ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

    IValueProvider IValueToken.Provider => Factory.Infix;
    protected abstract Declaration[] Declarations { get; }

    protected abstract Result GetResult(ContextBase context, Category category);

    protected abstract Result GetResult
        (ContextBase callContext, Category category, ValueSyntax? right, SourcePart token);

    internal virtual ValueSyntax? Visit(ISyntaxVisitor visitor)
    {
        NotImplementedMethod(visitor);
        return null;
    }
}

abstract class SuffixSyntaxToken : SuffixToken, ISuffix, IValueToken
{
    Result ISuffix.GetResult(ContextBase context, Category category, ValueSyntax left)
        => GetResult(context, category, left);

    IValueProvider IValueToken.Provider => Factory.Infix;

    protected abstract Result GetResult(ContextBase context, Category category, ValueSyntax left);
}

abstract class InfixSyntaxToken : InfixToken, IInfix, IValueToken
{
    Result IInfix.GetResult(ContextBase context, Category category, ValueSyntax left, ValueSyntax right)
        => GetResult(context, category, left, right);

    IValueProvider IValueToken.Provider => Factory.Infix;

    protected abstract Result GetResult
        (ContextBase callContext, Category category, ValueSyntax left, ValueSyntax right);
}