using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.ReniSyntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class ExpressionSyntax : CompileSyntax
    {
        internal ExpressionSyntax
            (Definable @operator, CompileSyntax left, SourcePart token, CompileSyntax right)
            : base(token)
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
        protected override ParsedSyntax[] Children => new ParsedSyntax[] { Left, Right };

        internal override string DumpPrintText
        {
            get
            {
                var result = base.GetNodeDump();
                if (Left != null)
                    result = "(" + Left.DumpPrintText + ")" + result;
                if (Right != null)
                    result += "(" + Right.DumpPrintText + ")";
                return result;
            }
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            if(Left == null)
                return context.PrefixResult(category, Token, Operator, Right);

            var typeForSearch = context.Type(Left).TypeForSearchProbes;
            var searchResults
                = typeForSearch
                    .DeclarationsForType(Operator)
                    .FilterLowerPriority()
                    .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    return UndefinedSymbolIssue.Type(Token, typeForSearch).IssueResult(category);

                case 1:
                    return searchResults[0].CallResult(context, category, Left, Right);

                default:
                    return AmbiguousSymbolIssue.Type(Token, context.RootContext).IssueResult(category);
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