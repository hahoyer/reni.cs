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
            StopByObjectIds(61, 62);
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

            var typeForSearch = left.Type;
            var searchResults
                = typeForSearch
                    .DeclarationsForTypeAndCloseRelatives(Definable)
                    .RemoveLowPriorityResults()
                    .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    return typeForSearch
                        .UndefinedSymbol(Token)
                        .Result(category);

                case 1:
                    return searchResults[0]
                        .Execute(category, left, context, Right);

                default:
                    return context
                        .RootContext
                        .UndefinedSymbol(Token)
                        .Result(category);
            }
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