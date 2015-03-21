using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass, ITerminal
    {
        protected override sealed Syntax Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix,token);

        public abstract Result Result(ContextBase context, Category category, TerminalSyntax token);

        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefix : TokenClass, ITerminal, ISuffix
    {
        protected override sealed Syntax Terminal(SourcePart token)
            => new TerminalSyntax(token.Id, this);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        public abstract Result Result(ContextBase context, Category category, TerminalSyntax token);

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
        protected override Syntax Terminal(SourcePart token) => new TerminalSyntax(token.Id, this);

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => new PrefixSyntax(this, right.ToCompiledSyntax);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token);

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

    abstract class SuffixToken : TokenClass, ISuffix
    {
        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
            => new SuffixSyntax(left.ToCompiledSyntax, this);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            =>  new CompileSyntaxError(IssueId.UnexpectedUseAsPrefix, token);

        protected override Syntax Terminal(SourcePart token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixToken : TokenClass, IInfix
    {
        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new InfixSyntax(left.ToCompiledSyntax, this, right.ToCompiledSyntax);

        protected override Syntax Terminal(SourcePart token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsTerminal, token);

        protected override Syntax Suffix(Syntax left, SourcePart token)
            => new CompileSyntaxError(IssueId.UnexpectedUseAsSuffix, token);

        public abstract Result Result
            (ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);
    }
}