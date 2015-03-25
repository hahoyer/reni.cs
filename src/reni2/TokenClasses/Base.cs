using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass
    {
        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);

        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);
    }

    abstract class NonPrefixToken : TokenClass
    {
        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);
    }

    abstract class NonSuffixToken : TokenClass
    {
        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);

        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);
    }

    abstract class SuffixToken : TokenClass
    {
        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsInfix.Syntax(token, left, right);

        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => IssueId.UnexpectedUseAsTerminal.Syntax(token);
    }

    abstract class InfixToken : TokenClass
    {
        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => IssueId.UnexpectedUseAsPrefix.Syntax(token, right);

        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => IssueId.UnexpectedUseAsTerminal.Syntax(token);

        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);
    }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal
    {
        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);

        public abstract Result Result(ContextBase context, Category category, TerminalSyntax token);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefixSyntaxToken : NonPrefixToken, ITerminal, ISuffix
    {
        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);
        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        public abstract Result Result(ContextBase context, Category category, TerminalSyntax token);

        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix
    {
        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);

        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => PrefixSyntax.Create(this, right.ToCompiledSyntax);

        public abstract Result Result(ContextBase context, Category category, TerminalSyntax token);

        public abstract Result Result
            (ContextBase context, Category category, PrefixSyntax token, CompileSyntax right);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class SuffixSyntaxToken : SuffixToken, ISuffix
    {
        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix
    {
        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, right.ToCompiledSyntax);

        public abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);
    }
}