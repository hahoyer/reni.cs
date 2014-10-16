using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    abstract class TerminalToken : TokenClass, ITerminal
    {
        protected override sealed ParsedSyntax TerminalSyntax(TokenData token) { return new TerminalSyntax(token, this); }
        public abstract Result Result(ContextBase context, Category category, TokenData token);
        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) { return Visit(visitor); }
        
        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonPrefix : TokenClass, ITerminal, ISuffix
    {
        protected override sealed ParsedSyntax TerminalSyntax(TokenData token) { return new TerminalSyntax(token, this); }
        protected override sealed ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token)
        {
            return new SuffixSyntax(token, left.ToCompiledSyntax(), this);
        }
        public abstract Result Result(ContextBase context, Category category, TokenData token);
        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) { return Visit(visitor); }

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class NonSuffix : TokenClass, ITerminal, IPrefix
    {
        protected override ParsedSyntax TerminalSyntax(TokenData token) { return new TerminalSyntax(token, this); }
        protected override ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right)
        {
            return new PrefixSyntax(token, this, right.ToCompiledSyntax());
        }
        public abstract Result Result(ContextBase context, Category category, TokenData token);
        public abstract Result Result(ContextBase context, Category category, TokenData token, CompileSyntax right);
        CompileSyntax ITerminal.Visit(ISyntaxVisitor visitor) { return Visit(visitor); }

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    abstract class Prefix : TokenClass, IPrefix
    {
        protected override ParsedSyntax PrefixSyntax(TokenData token, ParsedSyntax right)
        {
            return new PrefixSyntax(token, this, right.ToCompiledSyntax());
        }
        public abstract Result Result(ContextBase context, Category category, TokenData token, CompileSyntax right);
    }

    abstract class Suffix : TokenClass, ISuffix
    {
        protected override sealed ParsedSyntax SuffixSyntax(ParsedSyntax left, TokenData token)
        {
            return new SuffixSyntax(token, left.ToCompiledSyntax(), this);
        }
        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
    }

    abstract class Infix : TokenClass, IInfix
    {
        protected override sealed ParsedSyntax InfixSyntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            return new InfixSyntax
                (
                token
                ,
                left.ToCompiledSyntax()
                ,
                this
                ,
                right.ToCompiledSyntax()
                );
        }
        public abstract Result Result(ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);
    }
}