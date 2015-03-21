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

            var left = new ResultCache(c => context.ResultAsReference(c, Left));

            var typeForSearch = left.Type;
            var searchResults
                = typeForSearch
                    .DeclarationsForTypeAndCloseRelatives(Definable)
                    .RemoveLowPriorityResults()
                    .ToArray();

            switch(searchResults.Length)
            {
                case 0:
                    return typeForSearch.UndefinedSymbol(Token).Result(category);

                case 1:
                    return searchResults[0].Execute(category, left, context, Right);

                default:
                    return context.RootContext.UndefinedSymbol(Token)
                        .Result(category);
            }
        }

        internal override CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            var left = Left?.Visit(visitor);
            var right = Right?.Visit(visitor);
            if(left == null && right == null)
                return null;

            return (CompileSyntax) Definable.CreateForVisit(left ?? Left, right ?? Right);
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

        internal override Syntax CreateDeclarationSyntax(IToken token, Syntax right)
            => new CompileSyntaxError(IssueId.IdentifierExpected, token.Characters);

        internal override Syntax SyntaxError
            (IssueId issue, IToken token, Syntax right = null)
        {
            if(Right == null)
                return Left.SyntaxError(issue, token, right);
            NotImplementedMethod(issue, token);
            return null;
        }
    }

    // Lord of the weed
    // Katava dscho dscho
}