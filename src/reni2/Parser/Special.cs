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

        internal TerminalSyntax(ITerminal terminal, Syntax syntax)
            : base(syntax)
        {
            Terminal = terminal;
            StopByObjectIds();
        }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Terminal.Result(context, category, this);

        internal override Value Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        [DisableDump]
        internal SourcePart Token => Syntax.Token.Characters;

        protected override string GetNodeDump() => Terminal.NodeDump();
    }

    sealed class PrefixSyntax : Value
    {
        public static Result<Value> Create(IPrefix prefix, Result<Value> right, Syntax syntax)
            => new PrefixSyntax(prefix, right.Target,syntax).Issues<Value>(right.Issues);

        [Node]
        [EnableDump]
        readonly IPrefix Prefix;

        [Node]
        [EnableDump]
        readonly Value Right;

        public PrefixSyntax(IPrefix prefix, Value right, Syntax syntax)
            : base(syntax)
        {
            Prefix = prefix;
            Right = right;
        }


        internal override Result ResultForCache(ContextBase context, Category category) => Prefix
            .Result(context, category, Right, Syntax);

        protected override string GetNodeDump() => Prefix.NodeDump() + "(" + Right.NodeDump + ")";
    }

    sealed class InfixSyntax : Value
    {
        public static Result<Value> Create
            (Result<Value> left, IInfix infix, Result<Value> right, Syntax syntax)
        {
            Value value = new InfixSyntax(left.Target, infix, right.Target, syntax);
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

        public InfixSyntax(Value left, IInfix infix, Value right, Syntax syntax)
            : base(syntax)
        {
            Left = left;
            Infix = infix;
            Right = right;
            StopByObjectIds();
        }

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
            (Result<Value> left, ISuffix suffix, Syntax syntax)
        {
            Value value = new SuffixSyntax(left.Target, suffix, syntax);
            return value.Issues(left.Issues);
        }

        [Node]
        [EnableDump]
        readonly Value Left;

        [Node]
        [EnableDump]
        readonly ISuffix Suffix;

        internal SuffixSyntax(Value left, ISuffix suffix, Syntax syntax)
            : base(syntax)
        {
            Left = left;
            Suffix = suffix;
        }

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
        Result Result(ContextBase context, Category category, Value right, Syntax token);
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