using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExpressionSyntax : Value
    {
        internal sealed class EvaluationDepthExhaustedException : Exception
        {
            readonly ContextBase Context;
            readonly int Depth;
            readonly BinaryTree BinaryTree;

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

        internal static Result<Value> Create
            (BinaryTree parent, Value left, Definable definable, Value right)
            => new Result<Value>(new ExpressionSyntax(parent, left, definable, right));

        internal static Result<Value> Create(Definable definable, BinaryTree binaryTree, IValuesScope scope)
        {
            var leftValue = binaryTree.Left?.Value(scope);
            var rightValue = binaryTree.Right?.Value(scope);
            var left = leftValue?.Target;
            var right = rightValue?.Target;
            return Create(binaryTree, left, definable, right)
                .With(leftValue?.Issues.plus(rightValue?.Issues));
        }

        int CurrentResultDepth;

        ExpressionSyntax(BinaryTree parent, Value left, Definable definable, Value right)
            : base(parent)
        {
            Left = left;
            Definable = definable;
            Right = right;
            StopByObjectIds();
        }

        [Node]
        internal Value Left {get;}

        [Node]
        public Definable Definable {get;}

        [Node]
        internal Value Right {get;}

        protected override IEnumerable<Value> GetChildren() => T(Left,Right);

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(BinaryTree, context, CurrentResultDepth);

            try
            {
                CurrentResultDepth++;
                if(Left == null)
                    return context.PrefixResult(category, Definable, BinaryTree, Right);

                var left = context.ResultAsReferenceCache(Left);

                var leftType = left.Type;
                if(leftType == null)
                    return left.Issues.Result(category);

                if(leftType.HasIssues)
                    return leftType.Issues.Result(category);

                return leftType
                    .Execute(category, left, BinaryTree, Definable, context, Right);
            }
            finally
            {
                CurrentResultDepth--;
            }
        }

        internal override Value Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            var result = Definable.CreateForVisit(BinaryTree, left ?? Left, right ?? Right);
            Tracer.Assert(!result.Issues.Any());
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