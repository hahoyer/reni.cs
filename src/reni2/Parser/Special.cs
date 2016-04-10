using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    abstract class SpecialSyntax : Value {}

    sealed class TerminalSyntax : SpecialSyntax
    {
        internal string Id => Token.Id;
        internal override SourcePart Token { get; }

        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        internal TerminalSyntax(SourcePart token, ITerminal terminal)
        {
            Token = token;
            Terminal = terminal;
            StopByObjectIds();
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Terminal.Result(context, category, this);

        internal override Value Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        protected override string GetNodeDump() => Terminal.NodeDump();
    }

    sealed class PrefixSyntax : SpecialSyntax
    {
        public static Checked<OldSyntax> Create(IPrefix prefix, Checked<Value> right)
            => new PrefixSyntax(prefix, right.Value).Issues(right.Issues);

        [Node]
        [EnableDump]
        readonly IPrefix _prefix;

        [Node]
        [EnableDump]
        readonly Value _right;

        public PrefixSyntax(IPrefix prefix, Value right)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ResultForCache(ContextBase context, Category category) => _prefix
            .Result(context, category, this, _right);

        protected override string GetNodeDump() => _prefix.NodeDump() + "(" + _right.NodeDump + ")";
        protected override IEnumerable<OldSyntax> DirectChildren { get { yield return _right; } }
    }

    sealed class InfixSyntax : SpecialSyntax
    {
        public static Checked<OldSyntax> Create
            (
            Checked<Value> left,
            IInfix infix,
            SourcePart token,
            Checked<Value> right)
            => new InfixSyntax(left.Value, infix, token, right.Value)
                .Issues(left.Issues.plus(right.Issues));

        [Node]
        [EnableDump]
        readonly Value _left;

        [Node]
        [EnableDump]
        readonly IInfix _infix;

        [Node]
        [EnableDump]
        readonly Value _right;

        internal override SourcePart Token { get; }

        public InfixSyntax(Value left, IInfix infix, SourcePart token, Value right)
        {
            _left = left;
            _infix = infix;
            _right = right;
            Token = token;
            StopByObjectIds();
        }

        internal override IRecursionHandler RecursionHandler => _infix as IRecursionHandler;

        internal override Result ResultForCache(ContextBase context, Category category) => _infix
            .Result(context, category, _left, _right);

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
        protected override IEnumerable<OldSyntax> DirectChildren
        {
            get
            {
                yield return _left;
                yield return _right;
            }
        }

        internal override Checked<DeclaratorTags> Combine
            (DeclaratorTags syntax)
            => new Checked<DeclaratorTags>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));
    }

    interface IPendingProvider
    {
        Result Result
            (ContextBase context, Category category, Value left, Value right);
    }

    sealed class SuffixSyntax : SpecialSyntax
    {
        public static Checked<OldSyntax> Create
            (Checked<Value> left, ISuffix suffix, SourcePart token)
            => new SuffixSyntax(left.Value, suffix, token).Issues(left.Issues);

        [Node]
        [EnableDump]
        readonly Value _left;

        [Node]
        [EnableDump]
        readonly ISuffix _suffix;

        internal SuffixSyntax(Value left, ISuffix suffix, SourcePart token)
        {
            _left = left;
            _suffix = suffix;
            Token = token;
        }

        internal override SourcePart Token { get; }

        internal override Result ResultForCache(ContextBase context, Category category)
            => _suffix.Result(context, category, _left);

        protected override string GetNodeDump() => "(" + _left.NodeDump + ")" + _suffix;

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren { get { yield return _left; } }

        internal override Checked<DeclaratorTags> Combine
            (DeclaratorTags syntax)
            => new Checked<DeclaratorTags>
                (syntax, IssueId.UnexpectedDeclarationTag.CreateIssue(Token));
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, TerminalSyntax token);
        Value Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result
            (ContextBase context, Category category, PrefixSyntax token, Value right);
    }

    interface IInfix
    {
        Result Result
            (ContextBase context, Category category, Value left, Value right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, Value left);
    }
}