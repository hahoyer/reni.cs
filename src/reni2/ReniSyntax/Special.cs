using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(SourcePart token)
            : base(token) { }
    }

    sealed class TerminalSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        public TerminalSyntax(SourcePart token, ITerminal terminal)
            : base(token) { Terminal = terminal; }

        internal override string DumpPrintText { get { return Token.Name; } }
        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return Terminal
                .Result(context, category, Token);
        }

        internal override CompileSyntax Visit(ISyntaxVisitor visitor) { return Terminal.Visit(visitor); }
        public override IEnumerable<CompileSyntax> ToList(List type) { yield return ToCompiledSyntax(); }
    }

    sealed class PrefixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly IPrefix _prefix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        public PrefixSyntax(SourcePart token, IPrefix prefix, CompileSyntax right)
            : base(token)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _prefix
                .Result(context, category, Token, _right);
        }

        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + _right.NodeDump + ")"; }

        [DisableDump]
        protected override ParsedSyntax[] Children { get { return new Syntax[] {null, _right}; } }
    }

    sealed class InfixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly IInfix _infix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        public InfixSyntax(SourcePart token, CompileSyntax left, IInfix infix, CompileSyntax right)
            : base(token)
        {
            _left = left;
            _infix = infix;
            _right = right;
            StopByObjectId(12);
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _infix
                .Result(context, category, _left, _right);
        }

        internal override Result ObtainPendingResult(ContextBase context, Category category)
        {
            var pendingProvider = _infix as IPendingProvider;
            if(pendingProvider != null)
                return pendingProvider
                    .ObtainResult(context, category, _left, _right);
            return base.ObtainPendingResult(context, category);
        }

        protected override string GetNodeDump()
        {
            var result = "(";
            result += _left.NodeDump;
            result += ")";
            result += base.GetNodeDump();
            result += "(";
            result += _right.NodeDump;
            result += ")";
            return result;
        }

        [DisableDump]
        protected override ParsedSyntax[] Children { get { return new ParsedSyntax[] {_left, _right}; } }
    }

    interface IPendingProvider
    {
        Result ObtainResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    sealed class SuffixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly ISuffix _suffix;

        internal SuffixSyntax(SourcePart token, CompileSyntax left, ISuffix suffix)
            : base(token)
        {
            _left = left;
            _suffix = suffix;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _suffix
                .Result(context, category, _left);
        }

        protected override string GetNodeDump() { return "(" + _left.NodeDump + ")" + base.GetNodeDump(); }

        [DisableDump]
        protected override ParsedSyntax[] Children { get { return new ParsedSyntax[] {_left}; } }
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, SourcePart token);
        CompileSyntax Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, SourcePart token, CompileSyntax right);
    }

    interface IInfix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left);
    }
}