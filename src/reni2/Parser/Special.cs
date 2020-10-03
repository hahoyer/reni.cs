using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class TerminalSyntax : Syntax
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

        protected override IEnumerable<Syntax> GetChildren() => new Syntax[0];

        internal override Syntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        [DisableDump]
        internal SourcePart Token => BinaryTree.Token.Characters;

        protected override string GetNodeDump() => Terminal.NodeDump();
    }

    sealed class PrefixSyntax : Syntax
    {
        public static Result<Syntax> Create(IPrefix prefix, Result<Syntax> right, BinaryTree binaryTree)
            => new PrefixSyntax(prefix, right.Target,binaryTree).Issues<Syntax>(right.Issues);

        [Node]
        [EnableDump]
        readonly IPrefix Prefix;

        [Node]
        [EnableDump]
        readonly Syntax Right;

        public PrefixSyntax(IPrefix prefix, Syntax right, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Prefix = prefix;
            Right = right;
        }

        protected override IEnumerable<Syntax> GetChildren() => T(Right);

        internal override Result ResultForCache(ContextBase context, Category category) => Prefix
            .Result(context, category, Right, BinaryTree);

        protected override string GetNodeDump() => Prefix.NodeDump() + "(" + Right.NodeDump + ")";
    }

    sealed class InfixSyntax : Syntax
    {
        public static Result<Syntax> Create
            (Result<Syntax> left, IInfix infix, Result<Syntax> right, BinaryTree binaryTree)
        {
            Syntax syntax = new InfixSyntax(left.Target, infix, right.Target, binaryTree);
            return syntax.Issues(left.Issues.plus(right.Issues));
        }

        [Node]
        [EnableDump]
        readonly Syntax Left;

        [Node]
        [EnableDump]
        readonly IInfix Infix;

        [Node]
        [EnableDump]
        readonly Syntax Right;

        public InfixSyntax(Syntax left, IInfix infix, Syntax right, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Left = left;
            Infix = infix;
            Right = right;
            StopByObjectIds();
        }

        protected override IEnumerable<Syntax> GetChildren() => T(Left,Right);
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
            (ContextBase context, Category category, Syntax left, Syntax right);
    }

    sealed class SuffixSyntax : Syntax
    {
        public static Result<Syntax> Create
            (Result<Syntax> left, ISuffix suffix, BinaryTree binaryTree)
        {
            Syntax syntax = new SuffixSyntax(left.Target, suffix, binaryTree);
            return syntax.Issues(left.Issues);
        }

        [Node]
        [EnableDump]
        readonly Syntax Left;

        [Node]
        [EnableDump]
        readonly ISuffix Suffix;

        internal SuffixSyntax(Syntax left, ISuffix suffix, BinaryTree binaryTree)
            : base(binaryTree)
        {
            Left = left;
            Suffix = suffix;
        }

        protected override IEnumerable<Syntax> GetChildren() => T(Left);
        internal override Result ResultForCache(ContextBase context, Category category)
            => Suffix.Result(context, category, Left);

        protected override string GetNodeDump() => "(" + Left.NodeDump + ")" + Suffix;
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, TerminalSyntax token);
        Syntax Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, Syntax right, BinaryTree token);
    }

    interface IInfix
    {
        Result Result(ContextBase context, Category category, Syntax left, Syntax right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, Syntax left);
    }
}