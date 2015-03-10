using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass, ITerminal
    {
        protected override sealed Syntax Terminal(Token token)
            => new TerminalSyntax(token, this);

        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token)
                .SurroundCompileSyntax(left, right);

        public abstract Result Result(ContextBase context, Category category, Token token);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefix : TokenClass, ITerminal, ISuffix
    {
        protected override sealed Syntax Terminal(Token token)
            => new TerminalSyntax(token, this);

        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsPrefix, token)
                .SurroundCompileSyntax(left, right);

        public abstract Result Result(ContextBase context, Category category, Token token);

        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonSuffix : TokenClass, ITerminal, IPrefix
    {
        protected override Syntax Terminal(Token token) => new TerminalSyntax(token, this);

        protected override Syntax Prefix(Token token, Syntax right)
            => new PrefixSyntax(token, this, right.ToCompiledSyntax);

        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token)
                .SurroundCompileSyntax(left, right);

        public abstract Result Result(ContextBase context, Category category, Token token);

        public abstract Result Result
            (ContextBase context, Category category, Token token, CompileSyntax right);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class SuffixToken : TokenClass, ISuffix
    {
        protected override sealed Syntax Suffix(Syntax left, Token token)
            => new SuffixSyntax(token, left.ToCompiledSyntax, this);

        protected override Syntax Infix(Syntax left, Token token, Syntax right)
            =>
                new CompileSyntaxError(IssueId.UnexpectedUseAsPrefix, token)
                    .SurroundCompileSyntax(left, right);

        protected override Syntax Terminal(Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixToken : TokenClass, IInfix
    {
        protected override sealed Syntax Infix(Syntax left, Token token, Syntax right)
            => new InfixSyntax(token, left.ToCompiledSyntax, this, right.ToCompiledSyntax);

        protected override Syntax Terminal(Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        protected override Syntax Suffix(Syntax left, Token token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token)
            .Surround(left);

        public abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);
    }
}