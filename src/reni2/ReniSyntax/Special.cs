using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.ReniSyntax
{
    abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(SourcePart token, SourcePart additionalSourcePart = null)
            : base(token, additionalSourcePart) {}
        internal override bool IsKeyword => !IsNumber && !IsText;
    }

    sealed class TerminalSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        public TerminalSyntax(SourcePart token, ITerminal terminal, SourcePart sourcePart = null)
            : base(token, sourcePart)
        {
            Terminal = terminal;
        }

        internal override Result ResultForCache(ContextBase context, Category category) => Terminal
            .Result(context, category, Token);

        internal override CompileSyntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);
        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new TerminalSyntax(Token, Terminal, SourcePart + sourcePart);

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
            (SourcePart token, IPrefix prefix, CompileSyntax right, SourcePart sourcePart = null)
            : base(token, sourcePart)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ResultForCache(ContextBase context, Category category) => _prefix
            .Result(context, category, Token, _right);

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + _right.NodeDump + ")";

        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {null, _right};

        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new PrefixSyntax(Token, _prefix, _right, sourcePart);
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

        public InfixSyntax
            (
            SourcePart token,
            CompileSyntax left,
            IInfix infix,
            CompileSyntax right,
            SourcePart sourcePart = null)
            : base(token, sourcePart)
        {
            _left = left;
            _infix = infix;
            _right = right;
            StopByObjectId(12);
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

        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {_left, _right};

        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new InfixSyntax(Token, _left, _infix, _right, sourcePart);
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

        internal SuffixSyntax
            (
            SourcePart token,
            CompileSyntax left,
            ISuffix suffix,
            SourcePart sourcePart = null)
            : base(token, sourcePart)
        {
            _left = left;
            _suffix = suffix;
        }

        internal override Result ResultForCache(ContextBase context, Category category) => _suffix
            .Result(context, category, _left);

        protected override string GetNodeDump() => "(" + _left.NodeDump + ")" + base.GetNodeDump();

        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {_left};

        public override CompileSyntax Sourround(SourcePart sourcePart)
            => new SuffixSyntax(Token, _left, _suffix, sourcePart);
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
        Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left);
    }
}