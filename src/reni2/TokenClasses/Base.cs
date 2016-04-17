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
        protected sealed override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected sealed override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);

    }

    abstract class NonPrefixToken : TokenClass
    {
        protected sealed override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);
    }

    abstract class InfixPrefixToken : TokenClass
    {
        protected sealed override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);

    }

    abstract class NonSuffixToken : TokenClass
    {
        protected sealed override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);
    }

    abstract class SuffixToken : TokenClass
    {
        protected sealed override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

    }

    abstract class InfixToken : TokenClass
    {
        protected sealed override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected sealed override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token,left);

    }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal
    {
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

    abstract class NonPrefixSyntaxToken : NonPrefixToken, ITerminal, ISuffix
    {
        protected sealed override Result<OldSyntax> OldTerminal(SourcePart token)
            => new TerminalSyntax(token, this);

        protected override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        Result ISuffix.Result(ContextBase context, Category category, Value left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, Value left);

        Value ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual Value Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix
    {
        protected sealed override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => PrefixSyntax.Create(this, right.ToCompiledSyntax);

        protected sealed override Result<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

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
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix
    {
        protected sealed override Result<OldSyntax> OldTerminal(SourcePart token)
            => new TerminalSyntax(token, this);

        protected sealed override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => PrefixSyntax.Create(this, right.ToCompiledSyntax);

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
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix
    {
        protected sealed override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        Result ISuffix.Result(ContextBase context, Category category, Value left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, Value left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix
    {
        protected sealed override Result<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

        Result IInfix.Result
            (ContextBase context, Category category, Value left, Value right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, Value left, Value right);

    }
}