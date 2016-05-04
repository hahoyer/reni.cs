using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExpressionSyntax : Value
    {
        internal static Result<Value> Create
            (Value left, Definable definable, Value right, Syntax syntax)
            => new Result<Value>(new ExpressionSyntax(left, definable, right, syntax));

        internal static Result<Value> Create
            (Syntax left, Definable definable, Syntax right, Syntax syntax)
        {
            var leftvalue = left?.Value;
            var rightvalue = right?.Value;
            return new Result<Value>
                (
                new ExpressionSyntax(leftvalue?.Target, definable, rightvalue?.Target, syntax),
                leftvalue?.Issues.plus(rightvalue?.Issues)
                );
        }

        ExpressionSyntax
            (Value left, Definable definable, Value right, Syntax syntax)
            : base(syntax)
        {
            Left = left;
            Definable = definable;
            Right = right;
            StopByObjectIds();
        }

        [Node]
        internal Value Left { get; }
        [Node]
        public Definable Definable { get; }
        [Node]
        internal Value Right { get; }

        int CurrentResultDepth;

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

                return left.Type.Execute(category, left, Syntax, Definable, context, Right);
            }
            finally
            {
                CurrentResultDepth--;
            }
        }

        internal sealed class EvaluationDepthExhaustedException : Exception
        {
            readonly Syntax Syntax;
            readonly ContextBase Context;
            readonly int Depth;

            public EvaluationDepthExhaustedException(Syntax syntax, ContextBase context, int depth)
            {
                Syntax = syntax;
                Context = context;
                Depth = depth;
            }

            public override string Message
                => "Depth of " + Depth + " exhausted when evaluation expression.\n" +
                    "Expression: " + Syntax.SourcePart.GetDumpAroundCurrent(10) + "\n" +
                    "Context: " + Context.NodeDump;
        }

        internal override Value Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            var result = Definable.CreateForVisit(left ?? Left, right ?? Right, Syntax);
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