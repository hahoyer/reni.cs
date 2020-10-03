using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class TerminalSyntax : Value
    {
        internal string Id => Token.Id;

        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        internal TerminalSyntax(ITerminal terminal, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Terminal = terminal;
            StopByObjectIds();
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Terminal.Result(context, category, this);

        protected override IEnumerable<Value> GetChildren() => new Value[0];

        internal override Value Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        [DisableDump]
        internal SourcePart Token => BinaryTree.Token.Characters;

        protected override string GetNodeDump() => Terminal.NodeDump();
    }

    sealed class PrefixSyntax : Value
    {
        public static Result<Value> Create(IPrefix prefix, Result<Value> right, BinaryTree binaryTree)
            => new PrefixSyntax(prefix, right.Target,binaryTree).Issues<Value>(right.Issues);

        [Node]
        [EnableDump]
        readonly IPrefix Prefix;

        [Node]
        [EnableDump]
        readonly Value Right;

        public PrefixSyntax(IPrefix prefix, Value right, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Prefix = prefix;
            Right = right;
        }

        protected override IEnumerable<Value> GetChildren() => T(Right);

        internal override Result ResultForCache(ContextBase context, Category category) => Prefix
            .Result(context, category, Right, BinaryTree);

        protected override string GetNodeDump() => Prefix.NodeDump() + "(" + Right.NodeDump + ")";
    }

    sealed class InfixSyntax : Value
    {
        public static Result<Value> Create
            (Result<Value> left, IInfix infix, Result<Value> right, BinaryTree binaryTree)
        {
            Value value = new InfixSyntax(left.Target, infix, right.Target, binaryTree);
            return value.Issues(left.Issues.plus(right.Issues));
        }

        [Node]
        [EnableDump]
        readonly Value Left;

        [Node]
        [EnableDump]
        readonly IInfix Infix;

        [Node]
        [EnableDump]
        readonly Value Right;

        public InfixSyntax(Value left, IInfix infix, Value right, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Left = left;
            Infix = infix;
            Right = right;
            StopByObjectIds();
        }

        protected override IEnumerable<Value> GetChildren() => T(Left,Right);
        internal override IRecursionHandler RecursionHandler => Infix as IRecursionHandler;

        internal override Result ResultForCache(ContextBase context, Category category) => Infix
            .Result(context, category, Left, Right);

        protected override string GetNodeDump()
        {
            var result = "(";
            result += Left.NodeDump;
            result += ")";
            result += Infix.NodeDump();
            result += "(";
            result += Right.NodeDump;
            result += ")";
            return result;
        }
    }

    interface IPendingProvider
    {
        Result Result
            (ContextBase context, Category category, Value left, Value right);
    }

    sealed class SuffixSyntax : Value
    {
        public static Result<Value> Create
            (Result<Value> left, ISuffix suffix, BinaryTree binaryTree)
        {
            Value value = new SuffixSyntax(left.Target, suffix, binaryTree);
            return value.Issues(left.Issues);
        }

        [Node]
        [EnableDump]
        readonly Value Left;

        [Node]
        [EnableDump]
        readonly ISuffix Suffix;

        internal SuffixSyntax(Value left, ISuffix suffix, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Left = left;
            Suffix = suffix;
        }

        protected override IEnumerable<Value> GetChildren() => T(Left);
        internal override Result ResultForCache(ContextBase context, Category category)
            => Suffix.Result(context, category, Left);

        protected override string GetNodeDump() => "(" + Left.NodeDump + ")" + Suffix;
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, TerminalSyntax token);
        Value Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, Value right, BinaryTree token);
    }

    interface IInfix
    {
        Result Result(ContextBase context, Category category, Value left, Value right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, Value left);
    }
}