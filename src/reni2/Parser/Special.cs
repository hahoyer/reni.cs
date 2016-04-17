using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;

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

        internal override SourcePosn SourceStart => Token.Start;
        internal override SourcePosn SourceEnd => Token.End;

        internal override Value Visit(ISyntaxVisitor visitor) => Terminal.Visit(visitor);

        [DisableDump]
        internal long ToNumber => BitsConst.Convert(Id).ToInt64();

        protected override string GetNodeDump() => Terminal.NodeDump();
    }

    sealed class PrefixSyntax : SpecialSyntax
    {
        public static Result<Value> Create(IPrefix prefix, SourcePart token, Result<Value> right)
            => new PrefixSyntax(prefix, token, right.Target).Issues<Value>(right.Issues);

        [Node]
        [EnableDump]
        readonly IPrefix Prefix;

        [Node]
        [EnableDump]
        readonly Value Right;

        [DisableDump]
        internal override SourcePart Token { get; }

        public PrefixSyntax(IPrefix prefix, SourcePart token, Value right)
        {
            Prefix = prefix;
            Right = right;
            Token = token;
        }


        internal override Result ResultForCache(ContextBase context, Category category) => Prefix
            .Result(context, category, this, Right);

        protected override string GetNodeDump() => Prefix.NodeDump() + "(" + Right.NodeDump + ")";
        protected override IEnumerable<OldSyntax> DirectChildren { get { yield return Right; } }

        internal override SourcePosn SourceStart => Token.Start;
        internal override SourcePosn SourceEnd => Right.SourceEnd;
    }

    sealed class InfixSyntax : SpecialSyntax
    {
        public static Result<Value> Create
            (
            Result<Value> left,
            IInfix infix,
            SourcePart token,
            Result<Value> right)
        {
            Value value = new InfixSyntax(left.Target, infix, token, right.Target);
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

        internal override SourcePart Token { get; }

        public InfixSyntax(Value left, IInfix infix, SourcePart token, Value right)
        {
            Left = left;
            Infix = infix;
            Right = right;
            Token = token;
            StopByObjectIds();
        }

        internal override IRecursionHandler RecursionHandler => Infix as IRecursionHandler;

        internal override SourcePosn SourceStart => Left?.SourceStart ?? Token.Start;
        internal override SourcePosn SourceEnd => Right?.SourceEnd ?? Token.End;

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

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }
    }

    interface IPendingProvider
    {
        Result Result
            (ContextBase context, Category category, Value left, Value right);
    }

    sealed class SuffixSyntax : SpecialSyntax
    {
        public static Result<Value> Create
            (Result<Value> left, ISuffix suffix, SourcePart token)
        {
            Value value = new SuffixSyntax(left.Target, suffix, token);
            return value.Issues(left.Issues);
        }

        [Node]
        [EnableDump]
        readonly Value Left;

        [Node]
        [EnableDump]
        readonly ISuffix Suffix;

        internal SuffixSyntax(Value left, ISuffix suffix, SourcePart token)
        {
            Left = left;
            Suffix = suffix;
            Token = token;
        }

        internal override SourcePart Token { get; }

        internal override Result ResultForCache(ContextBase context, Category category)
            => Suffix.Result(context, category, Left);

        internal override SourcePosn SourceStart => Left?.SourceStart ?? Token.Start;
        internal override SourcePosn SourceEnd => Token.End;

        protected override string GetNodeDump() => "(" + Left.NodeDump + ")" + Suffix;

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren { get { yield return Left; } }
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