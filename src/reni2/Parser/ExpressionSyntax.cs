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
        internal static Checked<Value> Create
            (OldSyntax left, Definable definable, OldSyntax right, SourcePart token)
        {
            var left1 = left?.ToCompiledSyntax;
            var right1 = right?.ToCompiledSyntax;
            return new Checked<Value>
                (
                new ExpressionSyntax(left1?.Value, definable, right1?.Value, token),
                left1?.Issues.plus(right1?.Issues)
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

        internal override Checked<OldSyntax> RightSyntax(OldSyntax right, SourcePart token)
            => Checked<OldSyntax>
                .From
                (
                    Right == null
                        ? Create(Left, Definable, right, Token)
                        : Create(this, null, right, token)
                );

        internal override Checked<OldSyntax> InfixOfMatched(SourcePart token, OldSyntax right)
            => Checked<OldSyntax>.From(Create(this, null, right, token));

        int CurrentResultDepth = 0;
        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(CurrentResultDepth > 20)
                throw new EvaluationDepthExhaustedException(this, context, CurrentResultDepth);
            try
            {
                CurrentResultDepth++;
                if (Left == null)
                    return context.PrefixResult(category, Definable, Token, Right);

                var left = context.ResultAsReferenceCache(Left);

                return left.Type.Execute(category, left, Token, Definable, context, Right);
            }
            finally
            {
                CurrentResultDepth--;
            }
        }

        internal sealed class EvaluationDepthExhaustedException : Exception
        {
            readonly ExpressionSyntax Target;
            readonly ContextBase Context;
            readonly int Depth;

            public EvaluationDepthExhaustedException(ExpressionSyntax target, ContextBase context, int depth)
            {
                Target = target;
                Context = context;
                Depth = depth;
            }

            public override string Message 
                => "Depth of " + Depth + " exhausted when evaluation expression.\n" +
                "Expression: " + Target.SourcePart.GetDumpAroundCurrent(10) +"\n" +
                "Context: " + Context.NodeDump;
        }

        internal override Value Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            var result = Definable.CreateForVisit(left ?? Left, right ?? Right);
            Tracer.Assert(!result.Issues.Any());
            return (Value) result.Value;
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