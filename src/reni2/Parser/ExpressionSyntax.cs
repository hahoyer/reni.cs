using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExpressionSyntax : ValueSyntax
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

        internal static Result<ExpressionSyntax> Create
            (BinaryTree target, ValueSyntax left, Definable definable, ValueSyntax right)
            => new ExpressionSyntax(target, left, definable, right);

        internal static Result<ExpressionSyntax> Create(Definable definable, BinaryTree binaryTree, ISyntaxScope scope)
        {
            var leftValue = binaryTree.Left?.Syntax(scope);
            var rightValue = binaryTree.Right?.Syntax(scope);
            var left = leftValue?.Target;
            var right = rightValue?.Target;
            return Create(binaryTree, left, definable, right)
                .With(leftValue?.Issues.plus(rightValue?.Issues));
        }

        int CurrentResultDepth;

        ExpressionSyntax(BinaryTree target, ValueSyntax left, Definable definable, ValueSyntax right)
            : base(target)
        {
            Left = left;
            Definable = definable;
            Right = right;
            StopByObjectIds();
        }

        [Node]
        internal ValueSyntax Left {get;}

        [Node]
        public Definable Definable {get;}

        [Node]
        internal ValueSyntax Right {get;}

        protected override IEnumerable<Syntax> GetChildren() => T(Left,Right);
        internal override Result<CompoundSyntax> ToCompoundSyntax(BinaryTree target = null) 
            => new DeclarationSyntax(null, target, this).ToCompoundSyntax(null);

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(Target, context, CurrentResultDepth);

            try
            {
                CurrentResultDepth++;
                if(Left == null)
                    return context.PrefixResult(category, Definable, Target, Right);

                var left = context.ResultAsReferenceCache(Left);

                var leftType = left.Type;
                if(leftType == null)
                    return left.Issues.Result(category);

                if(leftType.HasIssues)
                    return leftType.Issues.Result(category);

                return leftType
                    .Execute(category, left, Target, Definable, context, Right);
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

            var result = Definable.CreateForVisit(Target, left ?? Left, right ?? Right);
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