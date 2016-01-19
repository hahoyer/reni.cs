using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
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

    abstract class InfixPrefixToken : TokenClass
    {
        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => IssueId.UnexpectedUseAsSuffix.Syntax(token, left);

        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => IssueId.UnexpectedUseAsTerminal.Syntax(token);
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
        internal override SourcePart Token { get; }

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

        Result ITerminal.Result(ContextBase context, Category category, TerminalSyntax token)
            => Result(context, category, token);

        protected abstract Result Result
            (ContextBase context, Category category, TerminalSyntax token);

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

    abstract class InfixPrefixSyntaxToken : InfixPrefixToken, IInfix,IPrefix
    {
        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
            => PrefixSyntax.Create(this, right.ToCompiledSyntax);

        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => Result(context, category, left, right);

        Result IPrefix.Result(ContextBase context, Category category, PrefixSyntax token, CompileSyntax right)
            => Result(context, category, right);

        protected abstract Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right);

        protected abstract Result Result
            (ContextBase context, Category category, CompileSyntax right);
    }

    abstract class NonSuffixSyntaxToken : NonSuffixToken, ITerminal, IPrefix
    {
        Checked<Syntax> ITerminal.LatePrefix(SourcePart token, Syntax right)
            => Prefix(token, right);

        protected override sealed Checked<Syntax> Terminal(SourcePart token)
            => new TerminalSyntax(token, this);

        protected override sealed Checked<Syntax> Prefix(SourcePart token, Syntax right)
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
        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => SuffixSyntax.Create(left.ToCompiledSyntax, this, token);

        Result ISuffix.Result(ContextBase context, Category category, CompileSyntax left)
            => Result(context, category, left);

        protected abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixSyntaxToken : InfixToken, IInfix
    {
        protected override sealed Checked<Syntax> Infix(Syntax left, SourcePart token, Syntax right)
            => InfixSyntax.Create(left.ToCompiledSyntax, this, token, right.ToCompiledSyntax);

        Result IInfix.Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
            => Result(context, category, left, right);

        protected abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);


        protected override sealed Checked<Syntax> Suffix(Syntax left, SourcePart token)
            => new InfixSyntaxCandidate(left, this, token);
    }
}