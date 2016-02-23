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
        protected sealed override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected sealed override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);

        protected sealed override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);
    }

    abstract class NonPrefixToken : TokenClass
    {
        protected sealed override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected sealed override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);
    }

    abstract class InfixPrefixToken : TokenClass
    {
        protected sealed override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);

        protected sealed override Checked<Syntax> Terminal(SourcePart token)
            => IssueId.UnexpectedUseAsTerminal.Syntax(token);
    }

    abstract class NonSuffixToken : TokenClass
    {
        protected sealed override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);

        protected sealed override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);
    }

    abstract class SuffixToken : TokenClass
    {
        protected sealed override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected sealed override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);

        protected sealed override Checked<Syntax> Terminal(SourcePart token)
            => IssueId.UnexpectedUseAsTerminal.Syntax(token);
    }

    abstract class InfixToken : TokenClass
    {
        protected sealed override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected sealed override Checked<Syntax> Terminal(SourcePart token)
            => IssueId.UnexpectedUseAsTerminal.Syntax(token);

        protected sealed override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token,left);

    }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal
    {
        protected sealed override Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefixSyntaxToken : NonPrefixToken, ITerminal, ISuffix
    {
        protected sealed override Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        Result ISuffix.Result(ContextBase context, Category category, CompileSyntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, CompileSyntax left);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix, IPrefix
    {
        protected sealed override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => PrefixSyntax.Create(this, right.ToCompiledSyntax);

        protected sealed override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

        Result IInfix.Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result
            (ContextBase context, Category category, PrefixSyntax token, CompileSyntax right)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right);

        protected abstract Result Result
            (ContextBase context, Category category, CompileSyntax right);
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix
    {
        protected sealed override Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

        protected sealed override Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => PrefixSyntax.Create(this, right.ToCompiledSyntax);

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

        Result IPrefix.Result
            (ContextBase context, Category category, PrefixSyntax token, CompileSyntax right)
            => Result(context, category, token, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, PrefixSyntax token, CompileSyntax right);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix
    {
        protected sealed override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        Result ISuffix.Result(ContextBase context, Category category, CompileSyntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix
    {
        protected sealed override Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

        Result IInfix.Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);

    }
}