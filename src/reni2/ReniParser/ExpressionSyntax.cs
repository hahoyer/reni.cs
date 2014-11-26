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
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class ExpressionSyntax : CompileSyntax
    {
        [Node]
        readonly Definable _tokenClass;
        [Node]
        internal readonly CompileSyntax Left;
        [Node]
        readonly SourcePart _token;
        [Node]
        internal readonly CompileSyntax Right;

        internal ExpressionSyntax
            (Definable tokenClass, CompileSyntax left, SourcePart token, CompileSyntax right)
            : base(token)
        {
            _tokenClass = tokenClass;
            Left = left;
            _token = token;
            Right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            if(Left == null)
                return context.ObtainResult(category, _token, _tokenClass, Right);

            var typeForSearch = context.Type(Left).TypeForSearchProbes;
            var searchResults
                = typeForSearch
                    .DeclarationsForType(_tokenClass)
                    .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    return UndefinedSymbolIssue.Type(_token, typeForSearch).IssueResult(category);

                case 1:
                    return searchResults[0].CallResult(context, category, Left, Right);

                default:
                    return AmbiguousSymbolIssue.Type(_token, context.RootContext).IssueResult(category);
            }
        }

        internal override CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            var left = Left == null ? null : Left.Visit(visitor);
            var right = Right == null ? null : Right.Visit(visitor);
            if(left == null && right == null)
                return this;

            return (CompileSyntax) _tokenClass.CreateForVisit(left ?? Left, _token, right ?? Right);
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

        [DisableDump]
        protected override ParsedSyntax[] Children { get { return new ParsedSyntax[] {Left, Right}; } }

        internal override string DumpPrintText
        {
            get
            {
                var result = base.GetNodeDump();
                if(Left != null)
                    result = "(" + Left.DumpPrintText + ")" + result;
                if(Right != null)
                    result += "(" + Right.DumpPrintText + ")";
                return result;
            }
        }

        internal override Syntax SyntaxError(IssueId issue, SourcePart token)
        {
            if(Right == null)
                return Left.SyntaxError(issue, token);
            NotImplementedMethod(issue,token);
            return null;

        }
    }

    // Lord of the weed
    // Katava dscho dscho
}