using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    [Serializable]
    internal abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(Token token)
            : base(token) { }
    }

    [Serializable]
    internal sealed class TerminalSyntax : SpecialSyntax
    {
        [Node, DumpData(true)]
        internal readonly ITerminal Terminal;

        public TerminalSyntax(Token token, ITerminal terminal)
            : base(token) { Terminal = terminal; }

        protected internal override Result Result(ContextBase context, Category category)
        {
            return Terminal
                .Result(context, category, Token);
        }
    }

    [Serializable]
    internal sealed class PrefixSyntax : SpecialSyntax
    {
        [Node, DumpData(true)]
        private readonly IPrefix _prefix;

        [Node, DumpData(true)]
        private readonly ICompileSyntax _right;

        public PrefixSyntax(Token token, IPrefix prefix, ICompileSyntax right)
            : base(token)
        {
            _prefix = prefix;
            _right = right;
        }

        protected internal override Result Result(ContextBase context, Category category)
        {
            return _prefix
                .Result(context, category, _right);
        }

        protected internal override string DumpShort() { return base.DumpShort() + "(" + _right.DumpShort() + ")"; }
    }

    [Serializable]
    internal sealed class InfixSyntax : SpecialSyntax
    {
        [Node, DumpData(true)]
        private readonly ICompileSyntax _left;

        [Node, DumpData(true)]
        private readonly IInfix _infix;

        [Node, DumpData(true)]
        private readonly ICompileSyntax _right;

        public InfixSyntax(Token token, ICompileSyntax left, IInfix infix, ICompileSyntax right)
            : base(token)
        {
            _left = left;
            _infix = infix;
            _right = right;
        }

        protected internal override Result Result(ContextBase context, Category category)
        {
            return _infix
                .Result(context, category, _left, _right);
        }

        protected internal override string DumpShort() { return "(" + _left.DumpShort() + ")" + base.DumpShort() + "(" + _right.DumpShort() + ")"; }
    }

    internal class SuffixSyntax : SpecialSyntax
    {
        [Node, DumpData(true)]
        private readonly ICompileSyntax _left;

        [Node, DumpData(true)]
        private readonly ISuffix _suffix;

        internal SuffixSyntax(Token token, ICompileSyntax left, ISuffix suffix)
            : base(token)
        {
            _left = left;
            _suffix = suffix;
        }

        protected internal override Result Result(ContextBase context, Category category)
        {
            return _suffix
                .Result(context, category, _left);
        }
    }

    internal interface ITerminal
    {
        Result Result(ContextBase context, Category category, Token token);
    }

    internal interface IPrefix
    {
        Result Result(ContextBase context, Category category, ICompileSyntax right);
    }

    internal interface IInfix
    {
        Result Result(ContextBase context, Category category, ICompileSyntax left, ICompileSyntax right);
    }

    internal interface ISuffix
    {
        Result Result(ContextBase context, Category category, ICompileSyntax left);
    }
}