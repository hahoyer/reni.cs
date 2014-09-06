using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Parser;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class ExpressionSyntax : CompileSyntax
    {
        [Node]
        readonly Defineable _tokenClass;
        [Node]
        internal readonly CompileSyntax Left;
        [Node]
        readonly TokenData _token;
        [Node]
        internal readonly CompileSyntax Right;

        internal ExpressionSyntax
            (Defineable tokenClass, CompileSyntax left, TokenData token, CompileSyntax right)
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
            {
                var searchResult
                    = context.DeclarationsForType(_tokenClass);

                if(searchResult != null)
                {
                    var result = searchResult.Data.FunctionResult(context, category, Right);
                    Tracer.Assert(category <= result.CompleteCategory);
                    return result;
                }
            }
            else
            {
                var searchResult
                    = context
                        .Type(Left)
                        .TypeForSearchProbes
                        .DeclarationsForType(_tokenClass);

                if(searchResult != null)
                    return searchResult.CallResult(context, category, Left, Right);
            }

            return UndefinedSymbolIssue.Type(context, this).IssueResult(category);
        }

        internal Probe[] Probes(ContextBase context) { throw new NotImplementedException(); }

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
        protected override ParsedSyntaxBase[] Children { get { return new ParsedSyntaxBase[] {Left, Right}; } }

        [DisableDump]
        internal override TokenData FirstToken { get { return Left == null ? Token : Left.FirstToken; } }

        [DisableDump]
        internal override TokenData LastToken { get { return Right == null ? Token : Right.LastToken; } }

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
    }

    // Lord of the weed
    // Katava dscho dscho
}