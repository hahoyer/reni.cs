using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExpressionSyntax : Value
    {
        internal static Result<Value> Create
            (Value left, Definable definable, Value right, SourcePart token)
            => new Result<Value>(new ExpressionSyntax(left, definable, right, token));

        internal static Result<Value> Create
            (Syntax left, Definable definable, Syntax right, SourcePart token)
        {
            var leftvalue = left?.Value;
            var rightvalue = right?.Value;
            return new Result<Value>
                (
                new ExpressionSyntax(leftvalue?.Target, definable, rightvalue?.Target, token),
                leftvalue?.Issues.plus(rightvalue?.Issues)
                );
        }

        ExpressionSyntax
            (
            Value left,
            Definable definable,
            Value right,
            SourcePart token)
        {
            Left = left;
            Definable = definable;
            Right = right;
            Token = token;
            StopByObjectIds();
        }

        [Node]
        internal Value Left { get; }
        [Node]
        public Definable Definable { get; }
        internal override SourcePart Token { get; }
        [Node]
        internal Value Right { get; }

        [DisableDump]
        protected override IEnumerable<OldSyntax> DirectChildren
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }

        int CurrentResultDepth;

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(this, context, CurrentResultDepth);

            try
            {
                CurrentResultDepth++;
                if(Left == null)
                    return context.PrefixResult(category, Definable, Token, Right);

                var left = context.ResultAsReferenceCache(Left);

                return left.Type.Execute(category, left, Token, Definable, context, Right);
            }
            finally
            {
                CurrentResultDepth--;
            }
        }

        internal override SourcePosn SourceStart => Left?.SourceStart ?? Token.Start;
        internal override SourcePosn SourceEnd => Right?.SourceEnd ?? Token.End;

        internal sealed class EvaluationDepthExhaustedException : Exception
        {
            readonly ExpressionSyntax Target;
            readonly ContextBase Context;
            readonly int Depth;

            public EvaluationDepthExhaustedException
                (ExpressionSyntax target, ContextBase context, int depth)
            {
                Target = target;
                Context = context;
                Depth = depth;
            }

            public override string Message
                => "Depth of " + Depth + " exhausted when evaluation expression.\n" +
                    "Expression: " + Target.SourcePart.GetDumpAroundCurrent(10) + "\n" +
                    "Context: " + Context.NodeDump;
        }

        internal override Value Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            var result = Definable.CreateForVisit(left ?? Left, right ?? Right, Token);
            Tracer.Assert(!result.Issues.Any());
            return result.Target;
        }

        internal override ResultCache.IResultProvider FindSource
            (IContextReference ext, ContextBase context)
        {
            var result = DirectChildren
                .OfType<Value>()
                .SelectMany(item => item.ResultCache)
                .Where(item => item.Key == context)
                .Select(item => item.Value)
                .FirstOrDefault(item => item.Exts.Contains(ext));

            if(result != null)
                return result.Provider;

            return GetDefinableResults(ext, context)
                .FirstOrDefault();
        }

        IEnumerable<ResultCache.IResultProvider> GetDefinableResults
            (IContextReference ext, ContextBase context)
        {
            if(Left == null)
                return context.GetDefinableResults(ext, Definable, Right);

            var left = context.ResultAsReferenceCache(Left);

            return left.Type.GetDefinableResults(ext, Definable, context, Right);
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