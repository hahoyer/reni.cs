using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Parser
{
    sealed class ExpressionSyntax : CompileSyntax
    {
        public static Checked<Syntax> Create
            (
            Checked<CompileSyntax> left,
            Definable definable,
            Checked<CompileSyntax> right,
            SourcePart token)
            => new ExpressionSyntax(left?.Value, definable, right?.Value, token)
                .Issues(left?.Issues.plus(right?.Issues));

        internal ExpressionSyntax
            (
            CompileSyntax left,
            Definable definable,
            CompileSyntax right,
            SourcePart token)
        {
            Left = left;
            Definable = definable;
            Right = right;
            Token = token;
            StopByObjectIds();
        }

        [Node]
        internal CompileSyntax Left { get; }
        [Node]
        public Definable Definable { get; }
        public SourcePart Token { get; }
        [Node]
        internal CompileSyntax Right { get; }

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return Left;
                yield return Right;
            }
        }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            if(Left == null)
                return context.PrefixResult(category, Definable, Token, Right);

            var left = context.ResultAsReferenceCache(Left);

            return left.Type.Execute(category, left, Token, Definable, context, Right);
        }

        internal override CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            var result = Definable.CreateForVisit(left ?? Left, right ?? Right);
            Tracer.Assert(!result.Issues.Any());
            return (CompileSyntax) result.Value;
        }

        protected override string GetNodeDump()
        {
            var result = Token.Id;
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