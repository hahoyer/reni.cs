using System;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
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
            readonly Syntax Syntax;

            public EvaluationDepthExhaustedException(Syntax syntax, ContextBase context, int depth)
            {
                Syntax = syntax;
                Context = context;
                Depth = depth;
            }

            public override string Message
                => "Depth of " +
                   Depth +
                   " exhausted when evaluation expression.\n" +
                   "Expression: " +
                   Syntax.Option.SourcePart.GetDumpAroundCurrent(10) +
                   "\n" +
                   "Context: " +
                   Context.NodeDump;
        }

        internal static Result<Value> Create
            (Syntax parent, Value left, Definable definable, Value right)
            => new Result<Value>(new ExpressionSyntax(parent, left, definable, right));

        internal static Result<Value> Create(Definable definable, Syntax syntax)
        {
            var leftvalue = syntax.Left?.Value;
            var rightvalue = syntax.Right?.Value;
            var left = leftvalue?.Target;
            var right = rightvalue?.Target;
            return Create(syntax, left, definable, right)
                .With(leftvalue?.Issues.plus(rightvalue?.Issues));
        }

        int CurrentResultDepth;

        ExpressionSyntax(Syntax parent, Value left, Definable definable, Value right)
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

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(Syntax, context, CurrentResultDepth);

            try
            {
                CurrentResultDepth++;
                if(Left == null)
                    return context.PrefixResult(category, Definable, Syntax, Right);

                var left = context.ResultAsReferenceCache(Left);

                var leftType = left.Type;
                if(leftType == null)
                    return left.Issues.Result(category);

                if(leftType.HasIssues)
                    return leftType.Issues.Result(category);

                return leftType
                    .Execute(category, left, Syntax, Definable, context, Right);
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

            var result = Definable.CreateForVisit(Syntax, left ?? Left, right ?? Right);
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