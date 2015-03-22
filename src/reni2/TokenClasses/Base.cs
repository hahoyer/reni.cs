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
        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsSuffix, token);

        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsSuffix, token);
    }

    abstract class NonPrefixToken : TokenClass
    {
        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);
    }

    abstract class NonSuffixToken : TokenClass
    {
        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsSuffix, token);

        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsSuffix, token);
    }

    abstract class SuffixToken : TokenClass
    {
        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override sealed Syntax Terminal(SourcePart token)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsTerminal, token);
    }

    abstract class InfixToken : TokenClass
    {
        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override sealed Syntax Terminal(SourcePart token)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
            => new Validation.SyntaxError(IssueId.UnexpectedUseAsSuffix, token);

    }

    abstract class TerminalSyntaxToken : TerminalToken, ITerminal
    {
        protected override sealed Syntax Terminal(SourcePart token)
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
        protected override sealed Syntax Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);
        protected override Syntax Suffix(Syntax left, SourcePart token)
            => new SuffixSyntax(left.ToCompiledSyntax, this);

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
        protected override sealed Syntax Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);

        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new PrefixSyntax(this, right.ToCompiledSyntax);

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
        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
            => new SuffixSyntax(left.ToCompiledSyntax, this);

        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix
    {
        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new InfixSyntax(left.ToCompiledSyntax, this, right.ToCompiledSyntax);

        public abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);
    }

}