using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExpressionSyntax : CompileSyntax
    {
        internal static Checked<CompileSyntax> Create
            (Syntax left, Definable definable, Syntax right, SourcePart token)
        {
            var left1 = left?.ToCompiledSyntax;
            var right1 = right?.ToCompiledSyntax;
            return new Checked<CompileSyntax>
                (
                new ExpressionSyntax(left1?.Value, definable, right1?.Value, token),
                left1?.Issues.plus(right1?.Issues)
                );
        }

        ExpressionSyntax
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

        internal override Checked<Syntax> RightSyntax(Syntax right, SourcePart token) 
            => Checked<Syntax>
            .From
            (
                Right == null
                    ? Create(Left, Definable, right, Token)
                    : Create(this, null, right, token)
            );

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