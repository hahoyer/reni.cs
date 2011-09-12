using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    [Serializable]
    internal abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(TokenData token)
            : base(token) { }
    }

    [Serializable]
    internal sealed class TerminalSyntax : SpecialSyntax
    {
        [Node, EnableDump]
        internal readonly ITerminal Terminal;

        public TerminalSyntax(TokenData token, ITerminal terminal)
            : base(token) { Terminal = terminal; }

        internal override string DumpPrintText { get { return Token.Name;  } }
        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return Terminal
                .Result(context, category, Token);
        }
        internal override bool? QuickIsDereferencedDataLess(ContextBase context) { return Terminal.QuickIsDereferencedDataLess(context, Token); }

        protected override TokenData GetFirstToken() { return Token; }
        protected override TokenData GetLastToken() { return Token; }
    }

    [Serializable]
    internal sealed class PrefixSyntax : SpecialSyntax
    {
        [Node, EnableDump]
        private readonly IPrefix _prefix;

        [Node, EnableDump]
        private readonly CompileSyntax _right;

        public PrefixSyntax(TokenData token, IPrefix prefix, CompileSyntax right)
            : base(token)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _prefix
                .Result(context, category, _right);
        }

        internal override string DumpShort() { return base.DumpShort() + "(" + _right.DumpShort() + ")"; }
        protected override TokenData GetFirstToken() { return Token; }
        protected override TokenData GetLastToken() { return _right.LastToken; }
    }

    [Serializable]
    internal sealed class InfixSyntax : SpecialSyntax
    {
        [Node, EnableDump]
        private readonly CompileSyntax _left;

        [Node, EnableDump]
        private readonly IInfix _infix;

        [Node, EnableDump]
        private readonly CompileSyntax _right;

        public InfixSyntax(TokenData token, CompileSyntax left, IInfix infix, CompileSyntax right)
            : base(token)
        {
            _left = left;
            _infix = infix;
            _right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _infix
                .Result(context, category, _left, _right);
        }

        internal override string DumpShort() { return "(" + _left.DumpShort() + ")" + base.DumpShort() + "(" + _right.DumpShort() + ")"; }
        protected override TokenData GetFirstToken() { return _left.FirstToken; }
        protected override TokenData GetLastToken() { return _right.LastToken; }
    }

    internal sealed class SuffixSyntax : SpecialSyntax
    {
        [Node, EnableDump]
        private readonly CompileSyntax _left;

        [Node, EnableDump]
        private readonly ISuffix _suffix;

        internal SuffixSyntax(TokenData token, CompileSyntax left, ISuffix suffix)
            : base(token)
        {
            _left = left;
            _suffix = suffix;
        }

        protected override bool GetIsLambda() { return _suffix is TokenClasses.Function; }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _suffix
                .Result(context, category, _left);
        }

        internal override string DumpShort() { return "(" + _left.DumpShort() + ")" + base.DumpShort(); }

        protected override TokenData GetFirstToken() { return _left.FirstToken; }
        protected override TokenData GetLastToken() { return Token; }
    }

    internal interface ITerminal
    {
        Result Result(ContextBase context, Category category, TokenData token);
        bool? QuickIsDereferencedDataLess(ContextBase context, TokenData token);
    }

    internal interface IPrefix
    {
        Result Result(ContextBase context, Category category, CompileSyntax right);
    }

    internal interface IInfix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    internal interface ISuffix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left);
    }
}