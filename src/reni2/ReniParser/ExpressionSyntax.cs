using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
            Definable @operator,
            CompileSyntax left,
            SourcePart token,
            CompileSyntax right,
            SourcePart sourcePart = null)
            : base(left?.SourcePart + token + right?.SourcePart + sourcePart, token)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        [Node]
        internal CompileSyntax Left { get; }
        [Node]
        public Definable Operator { get; }
        [Node]
        internal CompileSyntax Right { get; }

        [DisableDump]
        protected override ParsedSyntax[] Children
            => new ParsedSyntax[] {Left, Right}
                .Where(child => child != null)
                .ToArray();

        internal override Result ResultForCache(ContextBase context, Category category)
        {
            var @operator = Operator;

            if(Left == null)
                return context.PrefixResult(category, Token, @operator, Right);

            var left = new ResultCache(c => context.ResultAsReference(c, Left));

            var typeForSearch = left.Type;
            var searchResults
                = typeForSearch
                    .DeclarationsForTypeAndCloseRelatives(@operator)
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
                return this;

            return (CompileSyntax) Operator.CreateForVisit(left ?? Left, Token, right ?? Right);
        }
        public override CompileSyntax Sourround(SourcePart sourcePart) => new ExpressionSyntax(Operator, Left, Token, Right, sourcePart);

        protected override string GetNodeDump()
        {
            var result = base.GetNodeDump();
            if(Left != null)
                result = "(" + Left.NodeDump + ")" + result;
            if(Right != null)
                result += "(" + Right.NodeDump + ")";
            return result;
        }

        internal override Syntax SyntaxError(IssueId issue, SourcePart token)
        {
            if(Right == null)
                return Left.SyntaxError(issue, token);
            NotImplementedMethod(issue, token);
            return null;
        }
    }

    // Lord of the weed
    // Katava dscho dscho
}