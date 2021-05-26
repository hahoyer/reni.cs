using hw.DebugFormatter;
using hw.Parser;
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

        readonly IToken Token;

        internal TerminalSyntax(ITerminal terminal, IToken token, Anchor anchor)
            : base(anchor)
        {
            Terminal = terminal;
            Token = token;
            StopByObjectIds();
            Token.AssertIsNotNull();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Token.Characters.GetDumpAroundCurrent(5);

        [DisableDump]
        internal string Id => Token.Characters.Id;

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        internal override Result ResultForCache(ContextBase context, Category category)
            => Terminal.Result(context, category, Token);

        internal override ValueSyntax Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);
    }

    sealed class PrefixSyntax : ValueSyntax
    {
        [Node]
        readonly IPrefix Prefix;

        [Node]
        readonly ValueSyntax Right;

        readonly IToken Token;

        public PrefixSyntax(IPrefix prefix, ValueSyntax right, IToken token, Anchor brackets)
            : base(brackets)
        {
            Prefix = prefix;
            Right = right;
            Token = token;
            Token.AssertIsNotNull();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Token.Characters.GetDumpAroundCurrent(5);

        protected override int DirectChildCount => 1;

        protected override Syntax GetDirectChild(int index) => index == 0? Right : null;

        public static Result<ValueSyntax> Create
            (IPrefix prefix, Result<ValueSyntax> right, IToken token, Anchor brackets)
            => new PrefixSyntax(prefix, right.Target, token, brackets).AddIssues<ValueSyntax>(right.Issues);

        internal override Result ResultForCache(ContextBase context, Category category) 
            => Prefix.Result(context, category, Right, Token);
    }

    sealed class InfixSyntax : ValueSyntax
    {
        [Node]
        readonly IInfix Infix;

        [Node]
        internal readonly ValueSyntax Left;

        [Node]
        internal readonly ValueSyntax Right;

        readonly IToken Token;

        public InfixSyntax(ValueSyntax left, IInfix infix, ValueSyntax right, IToken token, Anchor brackets)
            : base(brackets)
        {
            Left = left;
            Infix = infix;
            Right = right;
            Token = token;
            Token.AssertIsNotNull();
            StopByObjectIds();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Token.Characters.GetDumpAroundCurrent(5);

        internal override IRecursionHandler RecursionHandler => Infix as IRecursionHandler;

        protected override int DirectChildCount => 2;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Left, 1 => Right, _ => null
            };

        public static Result<ValueSyntax> Create
        (
            Result<ValueSyntax> left, IInfix infix, Result<ValueSyntax> right, IToken token
            , Anchor brackets
        )
        {
            ValueSyntax syntax = new InfixSyntax(left.Target, infix, right.Target, token, brackets);
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
        internal readonly ValueSyntax Left;

        [Node]
        readonly ISuffix Suffix;

        readonly IToken Token;

        internal SuffixSyntax(ValueSyntax left, ISuffix suffix, IToken token, Anchor brackets)
            : base(brackets)
        {
            Left = left;
            Suffix = suffix;
            Token = token;
            Token.AssertIsNotNull();
        }

        [EnableDump]
        [EnableDumpExcept(null)]
        string Position => Token.Characters.GetDumpAroundCurrent(5);

        protected override int DirectChildCount => 1;

        protected override Syntax GetDirectChild(int index) => index == 0? Left : null;

        public static Result<ValueSyntax> Create
            (Result<ValueSyntax> left, ISuffix suffix, IToken token, Anchor brackets)
        {
            ValueSyntax syntax = new SuffixSyntax(left.Target, suffix, token, brackets);
            return syntax.AddIssues(left.Issues);
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Suffix.Result(context, category, Left);
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, IToken token);
        ValueSyntax Visit(ISyntaxVisitor visitor);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, ValueSyntax right, IToken token);
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