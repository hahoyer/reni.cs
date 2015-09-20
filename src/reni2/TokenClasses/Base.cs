﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
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

        protected override Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => new InfixSyntaxCandidate(left, this, token);

        public Checked<Syntax> LateInfix(Syntax left, SourcePart token, Syntax right)
            => Infix(left, token, right);
    }

    sealed class InfixSyntaxCandidate : Syntax
    {
        internal readonly Syntax Left;
        internal readonly InfixToken Class;
        internal readonly SourcePart Token;

        public InfixSyntaxCandidate(Syntax left, InfixToken @class, SourcePart token)
        {
            Left = left;
            Class = @class;
            Token = token;
        }

        [DisableDump]
        internal override Checked<CompileSyntax> ToCompiledSyntax
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal override Checked<ExclamationSyntaxList> Combine
            (ExclamationSyntaxList syntax)
            => new Checked<ExclamationSyntaxList>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));

        internal override Checked<Syntax> RightSyntax(Syntax right, SourcePart token)
            => Class.LateInfix(Left, Token, right);
    }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal
    {
        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

        public abstract Result Result(ContextBase context, Category category, TerminalSyntax token);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        Checked<Syntax> ITerminal.LatePrefix(SourcePart token, Syntax right)
            => Prefix(token, right);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefixSyntaxToken : NonPrefixToken, ITerminal, ISuffix
    {
        Checked<Syntax> ITerminal.LatePrefix(SourcePart token, Syntax right)
            => Prefix(token, right);

        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

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
        Checked<Syntax> ITerminal.LatePrefix(SourcePart token, Syntax right)
            => Prefix(token, right);

        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

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
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

        public abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);

        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => new InfixSyntaxCandidate(left, this, token);
    }
}