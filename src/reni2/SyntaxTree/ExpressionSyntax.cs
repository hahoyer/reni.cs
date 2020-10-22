using System;
using System.Linq;
using hw.DebugFormatter;
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
            readonly BinaryTree BinaryTree;
            readonly ContextBase Context;
            readonly int Depth;

            public EvaluationDepthExhaustedException(BinaryTree binaryTree, ContextBase context, int depth)
            {
                BinaryTree = binaryTree;
                Context = context;
                Depth = depth;
            }

            public override string Message
                => "Depth of " +
                   Depth +
                   " exhausted when evaluation expression.\n" +
                   "Expression: " +
                   BinaryTree.SourcePart.GetDumpAroundCurrent(10) +
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

        internal ExpressionSyntax
        (
            BinaryTree anchorLeft,
            BinaryTree anchor,
            ValueSyntax left,
            Definable definable,
            ValueSyntax right,
            BinaryTree anchorRight, FrameItemContainer frameItems
        )
            : base(anchorLeft, anchor, anchorRight, frameItems)
        {
            anchor.AssertIsNotNull();
            Left = left;
            Definable = definable;
            Right = right;
            StopByObjectIds();
        }

        [DisableDump]
        protected override int LeftDirectChildCountKernel => 1;
        [DisableDump]
        protected override int DirectChildCountKernel => 2;

        protected override Syntax GetDirectChildKernel(int index)
            => index switch
            {
                0 => Left
                , 1 => Right
                , _ => null
            };

        internal static Result<ExpressionSyntax> Create
            (BinaryTree target, ValueSyntax left, Definable definable, ValueSyntax right, FrameItemContainer frameItems)
            => new ExpressionSyntax(null, target, left, definable, right, null, frameItems);

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(Anchor, context, CurrentResultDepth);

            try
            {
                CurrentResultDepth++;
                if(Left == null)
                    return context.PrefixResult(category, Definable, Anchor, Right);

                var left = context.ResultAsReferenceCache(Left);

                var leftType = left.Type;
                if(leftType == null)
                    return left.Issues.Result(category);

                if(leftType.HasIssues)
                    return leftType.Issues.Result(category);

                return leftType
                    .Execute(category, left, Anchor, Definable, context, Right);
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

            var result = Definable.CreateForVisit(Anchor, left ?? Left, right ?? Right, FrameItems);
            (!result.Issues.Any()).Assert();
            return result.Target;
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