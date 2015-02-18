using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass, ITerminal
    {
        protected override sealed Syntax Terminal(SourcePart token) => new TerminalSyntax(token, this);
        public abstract Result Result(ContextBase context, Category category, SourcePart token);
        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefix : TokenClass, ITerminal, ISuffix
    {
        protected override sealed Syntax Terminal(SourcePart token) => new TerminalSyntax(token, this);
        public abstract Result Result(ContextBase context, Category category, SourcePart token);
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
        protected override Syntax Terminal(SourcePart token) => new TerminalSyntax(token, this);
        protected override Syntax Prefix(SourcePart token, Syntax right) => new PrefixSyntax(token, this, right.ToCompiledSyntax);
        public abstract Result Result(ContextBase context, Category category, SourcePart token);
        public abstract Result Result(ContextBase context, Category category, SourcePart token, CompileSyntax right);
        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) => Visit(visitor);

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class SuffixToken : TokenClass, ISuffix
    {
        protected override sealed Syntax Suffix(Syntax left, SourcePart token) => new SuffixSyntax(token, left.ToCompiledSyntax, this);
        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class InfixToken : TokenClass, IInfix
    {
        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right) => new InfixSyntax(token, left.ToCompiledSyntax, this, right.ToCompiledSyntax);
        public abstract Result Result(ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);
    }
}