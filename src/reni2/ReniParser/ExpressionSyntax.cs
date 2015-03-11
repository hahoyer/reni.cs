using System;
using System.Collections.Generic;
using System.Linq;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.ReniSyntax;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class ExpressionSyntax : CompileSyntax
    {
        internal ExpressionSyntax
            (
            CompileSyntax left,
            DefinableTokenSyntax @operator,
            CompileSyntax right)
            : base(@operator.Token)
        {
            Operator = @operator;
            Left = left;
            Right = right;
            StopByObjectIds();
        }

        ExpressionSyntax(ExpressionSyntax other, ParsedSyntax[] parts)
            : base(other, parts)
        {
            Operator = other.Operator;
            Left = other.Left;
            Right = other.Right;
        }

        [Node]
        internal CompileSyntax Left { get; }
        [Node]
        public DefinableTokenSyntax Operator { get; }
        [Node]
        internal CompileSyntax Right { get; }

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var @operator = Operator;

            if(Left == null)
                return context.PrefixResult(category, Token, @operator.Definable, Right);

            var left = new ResultCache(c => context.ResultAsReference(c, Left));

            var typeForSearch = left.Type;
            var searchResults
                = typeForSearch
                    .DeclarationsForTypeAndCloseRelatives(@operator.Definable)
                    .RemoveLowPriorityResults()
                    .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    return UndefinedSymbolIssue.Type(Token, typeForSearch).IssueResult(category);

                case 1:
                    return searchResults[0].Execute(category, left, context, Right);

                default:
                    return AmbiguousSymbolIssue.Type(Token, context.RootContext)
                        .IssueResult(category);
            }
        }

        internal override CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            return (CompileSyntax) Operator.Definable.CreateForVisit(left ?? Left, Token, right ?? Right);
        }
        internal override CompileSyntax SurroundCompileSyntax(params ParsedSyntax[] parts)
            => new ExpressionSyntax(this, parts);

        protected override string GetNodeDump()
        {
            var result = base.GetNodeDump();
            if(Left != null)
                result = "(" + Left.NodeDump + ")" + result;
            if(Right != null)
                result += "(" + Right.NodeDump + ")";
            return result;
        }

        internal override Syntax CreateDeclarationSyntax(Token token, Syntax right)
            => new CompileSyntaxError(IssueId.IdentifyerExpected, Token)
                .SurroundCompileSyntax(this, right);

        internal override Syntax SyntaxError
            (IssueId issue, Token token, Syntax right = null, params ParsedSyntax[] parts)
        {
            if(Right == null)
                return Left.SyntaxError(issue, token, right, parts);
            NotImplementedMethod(issue, token);
            return null;
        }

        protected override IEnumerable<Syntax> SyntaxChildren
        {
            get
            {
                yield return Left;
                yield return Operator;
                yield return Right;
            }
        }

    }

    // Lord of the weed
    // Katava dscho dscho
}