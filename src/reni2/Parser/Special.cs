using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    abstract class SpecialSyntax : CompileSyntax {}

    sealed class TerminalSyntax : SpecialSyntax
    {
        internal string Id => Token.Id;
        SourcePart Token => SourcePart;

        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        internal TerminalSyntax(SourcePart token, ITerminal terminal)
        {
            SourcePart = token;
            Terminal = terminal;
            StopByObjectIds();
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Terminal.Result(context, category, this);

        internal override CompileSyntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        protected override string GetNodeDump() => Terminal.NodeDump();

        internal override Checked<Syntax> RightSyntax(Syntax right, SourcePart token)
            => Terminal.LatePrefix(Token, right);
    }

    sealed class PrefixSyntax : SpecialSyntax
    {
        public static Checked<Syntax> Create(IPrefix prefix, Checked<CompileSyntax> right)
            => new PrefixSyntax(prefix, right.Value).Issues(right.Issues);

        [Node]
        [EnableDump]
        readonly IPrefix _prefix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        public PrefixSyntax(IPrefix prefix, CompileSyntax right)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ResultForCache(ContextBase context, Category category) => _prefix
            .Result(context, category, this, _right);

        protected override string GetNodeDump() => _prefix.NodeDump() + "(" + _right.NodeDump + ")";
        protected override IEnumerable<Syntax> DirectChildren { get { yield return _right; } }
    }

    sealed class InfixSyntax : SpecialSyntax
    {
        public static Checked<Syntax> Create
            (
            Checked<CompileSyntax> left,
            IInfix infix,
            SourcePart token,
            Checked<CompileSyntax> right)
            => new InfixSyntax(left.Value, infix, token, right.Value)
                .Issues(left.Issues.plus(right.Issues));

        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly IInfix _infix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        SourcePart Token { get; }

        public InfixSyntax(CompileSyntax left, IInfix infix, SourcePart token, CompileSyntax right)
        {
            _left = left;
            _infix = infix;
            _right = right;
            Token = token;
            StopByObjectIds();
        }

        public override IRecursionHandler RecursionHandler => _infix as IRecursionHandler;

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
            result += _infix.NodeDump();
            result += "(";
            result += _right.NodeDump;
            result += ")";
            return result;
        }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return _left;
                yield return _right;
            }
        }

        internal override Checked<ExclamationSyntaxList> Combine
            (ExclamationSyntaxList syntax)
            => new Checked<ExclamationSyntaxList>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));
    }

    interface IPendingProvider
    {
        Result Result
            (ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    sealed class SuffixSyntax : SpecialSyntax
    {
        public static Checked<Syntax> Create
            (Checked<CompileSyntax> left, ISuffix suffix, SourcePart token)
            => new SuffixSyntax(left.Value, suffix, token).Issues(left.Issues);

        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly ISuffix _suffix;

        internal SuffixSyntax(CompileSyntax left, ISuffix suffix, SourcePart token)
        {
            _left = left;
            _suffix = suffix;
            Token = token;
        }

        SourcePart Token { get; }

        internal override Result ResultForCache(ContextBase context, Category category)
            => _suffix.Result(context, category, _left);

        protected override string GetNodeDump() => "(" + _left.NodeDump + ")" + _suffix;

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren { get { yield return _left; } }

        internal override Checked<ExclamationSyntaxList> Combine
            (ExclamationSyntaxList syntax)
            => new Checked<ExclamationSyntaxList>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, TerminalSyntax token);
        CompileSyntax Visit(ISyntaxVisitor visitor);
        Checked<Syntax> LatePrefix(SourcePart token, Syntax right);
    }

    interface IPrefix
    {
        Result Result
            (ContextBase context, Category category, PrefixSyntax token, CompileSyntax right);
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