using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(IToken token)
            : base(token) { }

        internal override bool IsKeyword => !IsNumber && !IsText;
    }

    sealed class TerminalSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        public TerminalSyntax(IToken token, ITerminal terminal)
            : base(token) { Terminal = terminal; }

        internal override Result ResultForCache(ContextBase context, Category category) => Terminal
            .Result(context, category, Token);

        internal override CompileSyntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        internal override bool IsNumber => Terminal is Number;
        internal override bool IsText => Terminal is Text;
    }

    sealed class PrefixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly IPrefix _prefix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        public PrefixSyntax
            (IToken token, IPrefix prefix, CompileSyntax right)
            : base(token)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ResultForCache(ContextBase context, Category category) => _prefix
            .Result(context, category, Token, _right);

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + _right.NodeDump + ")";
        protected override IEnumerable<Syntax> DirectChildren { get { yield return _right; } }
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

        public InfixSyntax(IToken token, CompileSyntax left, IInfix infix, CompileSyntax right)
            : base(token)
        {
            _left = left;
            _infix = infix;
            _right = right;
            StopByObjectIds();
        }

        internal override Result ResultForCache(ContextBase context, Category category) => _infix
            .Result(context, category, _left, _right);

        internal override Result PendingResultForCache(ContextBase context, Category category)
        {
            var pendingProvider = _infix as IPendingProvider;
            if(pendingProvider != null)
                return pendingProvider
                    .Result(context, category, _left, _right);
            return base.PendingResultForCache(context, category);
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

        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return _left;
                yield return _right;
            }
        }
    }

    interface IPendingProvider
    {
        Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    sealed class SuffixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly ISuffix _suffix;

        internal SuffixSyntax(IToken token, CompileSyntax left, ISuffix suffix)
            : base(token)
        {
            _left = left;
            _suffix = suffix;
        }
        internal override Result ResultForCache(ContextBase context, Category category) => _suffix
            .Result(context, category, _left);

        protected override string GetNodeDump() => "(" + _left.NodeDump + ")" + base.GetNodeDump();
        protected override IEnumerable<Syntax> DirectChildren { get { yield return _left; } }
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, IToken token);
        CompileSyntax Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, IToken token, CompileSyntax right);
    }

    interface IInfix
    {
        Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left);
    }
}