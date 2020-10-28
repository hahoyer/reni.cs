using System;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    sealed class ExpressionSyntax : ValueSyntax
    {
        internal sealed class EvaluationDepthExhaustedException : Exception
        {
            readonly ContextBase Context;
            readonly int Depth;
            readonly SourcePart SourcePart;

            public EvaluationDepthExhaustedException(SourcePart sourcePart, ContextBase context, int depth)
            {
                SourcePart = sourcePart;
                Context = context;
                Depth = depth;
            }

            public override string Message
                => "Depth of " +
                   Depth +
                   " exhausted when evaluation expression.\n" +
                   "Expression: " +
                   SourcePart.GetDumpAroundCurrent(10) +
                   "\n" +
                   "Context: " +
                   Context.NodeDump;
        }

        [Node]
        public Definable Definable { get; }

        [Node]
        [EnableDumpExcept(null)]
        internal ValueSyntax Left { get; }

        [Node]
        [EnableDumpExcept(null)]
        internal ValueSyntax Right { get; }

        int CurrentResultDepth;
        IToken Token;

        internal ExpressionSyntax(ValueSyntax left, Definable definable, IToken token, ValueSyntax right, Anchor anchor)
            : base(anchor)
        {
            Token = token;
            Left = left;
            Definable = definable;
            Right = right;
            StopByObjectIds();
        }

        [DisableDump]
        protected override int LeftDirectChildCountInternal => 1;

        [DisableDump]
        protected override int DirectChildCount => 2;

        protected override Syntax GetDirectChild(int index)
            => index switch
            {
                0 => Left, 1 => Right, _ => null
            };

        internal static ExpressionSyntax Create
        (
            ValueSyntax left,
            Definable definable,
            IToken token,
            ValueSyntax right,
            Anchor frameItems
        )
            => new ExpressionSyntax(left, definable, token, right, frameItems);

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(Anchor.SourceParts.Combine(), context, CurrentResultDepth);

            try
            {
                CurrentResultDepth++;
                if(Left == null)
                    return context.PrefixResult(category, Definable, Token, Right);

                var left = context.ResultAsReferenceCache(Left);

                var leftType = left.Type;
                if(leftType == null)
                    return left.Issues.Result(category);

                if(leftType.HasIssues)
                    return leftType.Issues.Result(category);

                return leftType
                    .Execute(category, left, Token, Definable, context, Right);
            }
            finally
            {
                CurrentResultDepth--;
            }
        }

        internal override ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            return Definable.CreateForVisit(left ?? Left, right ?? Right, Anchor, Token);
        }

        protected override string GetNodeDump()
        {
            var result = Definable?.Id ?? "";
            if(Left != null)
                result = "(" + Left.NodeDump + ")" + result;
            if(Right != null)
                result += "(" + Right.NodeDump + ")";
            return result;
        }
    }

    // Lord of the weed
    // Katava dscho dscho
}