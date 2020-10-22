using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class TerminalSyntax : ValueSyntax.NoChildren
    {
        [Node]
        [DisableDump]
        internal readonly ITerminal Terminal;

        internal TerminalSyntax(ITerminal terminal, BinaryTree anchor, FrameItemContainer frameItems)
            : base(anchor, frameItems)
        {
            Terminal = terminal;
            StopByObjectIds();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Token.GetDumpAroundCurrent(5);

        internal string Id => Token.Id;

        internal SourcePart Token => Anchor.Token.Characters;

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        internal override Result ResultForCache(ContextBase context, Category category)
            => Terminal.Result(context, category, this);

        internal override ValueSyntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);
    }

    sealed class PrefixSyntax : ValueSyntax
    {
        [Node]
        readonly IPrefix Prefix;

        [Node]
        readonly ValueSyntax Right;

        public PrefixSyntax(IPrefix prefix, ValueSyntax right, BinaryTree anchor, FrameItemContainer brackets)
            : base(null, anchor, null, brackets)
        {
            anchor.AssertIsNotNull();
            Prefix = prefix;
            Right = right;
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

        protected override int LeftDirectChildCountKernel => 0;
        protected override int DirectChildCountKernel => 1;

        protected override Syntax GetDirectChildKernel(int index) => index == 0? Right : null;

        public static Result<ValueSyntax> Create
            (IPrefix prefix, Result<ValueSyntax> right, BinaryTree binaryTree, FrameItemContainer brackets)
            => new PrefixSyntax(prefix, right.Target, binaryTree, brackets).AddIssues<ValueSyntax>(right.Issues);

        internal override Result ResultForCache(ContextBase context, Category category) => Prefix
            .Result(context, category, Right, Anchor);
    }

    sealed class InfixSyntax : ValueSyntax
    {
        [Node]
        readonly IInfix Infix;

        [Node]
        readonly ValueSyntax Left;

        [Node]
        readonly ValueSyntax Right;

        public InfixSyntax(ValueSyntax left, IInfix infix, ValueSyntax right, BinaryTree anchor, FrameItemContainer brackets)
            : base(null, anchor, null, brackets)
        {
            anchor.AssertIsNotNull();
            Left = left;
            Infix = infix;
            Right = right;
            StopByObjectIds();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

        internal override IRecursionHandler RecursionHandler => Infix as IRecursionHandler;

        protected override int LeftDirectChildCountKernel => 1;
        protected override int DirectChildCountKernel => 2;

        protected override Syntax GetDirectChildKernel(int index)
            => index switch
            {
                0 => Left, 1 => Right, _ => null
            };

        public static Result<ValueSyntax> Create
        (
            Result<ValueSyntax> left, IInfix infix, Result<ValueSyntax> right, BinaryTree binaryTree
            , FrameItemContainer brackets
        )
        {
            ValueSyntax syntax = new InfixSyntax(left.Target, infix, right.Target, binaryTree, brackets);
            return syntax.AddIssues(left.Issues.plus(right.Issues));
        }

        internal override Result ResultForCache(ContextBase context, Category category) => Infix
            .Result(context, category, Left, Right);
    }

    interface IPendingProvider
    {
        Result Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right);
    }

    sealed class SuffixSyntax : ValueSyntax
    {
        [Node]
        readonly ValueSyntax Left;

        [Node]
        readonly ISuffix Suffix;

        internal SuffixSyntax(ValueSyntax left, ISuffix suffix, BinaryTree anchor, FrameItemContainer brackets)
            : base(null,anchor, null, brackets)
        {
            anchor.AssertIsNotNull();
            Left = left;
            Suffix = suffix;
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Anchor?.Token.Characters.GetDumpAroundCurrent(5);

        protected override int LeftDirectChildCountKernel => 1;
        protected override int DirectChildCountKernel => 1;

        protected override Syntax GetDirectChildKernel(int index) => index == 0? Left : null;

        public static Result<ValueSyntax> Create
            (Result<ValueSyntax> left, ISuffix suffix, BinaryTree binaryTree, FrameItemContainer brackets)
        {
            ValueSyntax syntax = new SuffixSyntax(left.Target, suffix, binaryTree, brackets);
            return syntax.AddIssues(left.Issues);
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Suffix.Result(context, category, Left);
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, TerminalSyntax token);
        ValueSyntax Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, ValueSyntax right, BinaryTree token);
    }

    interface IInfix
    {
        Result Result(ContextBase context, Category category, ValueSyntax left, ValueSyntax right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, ValueSyntax left);
    }
}