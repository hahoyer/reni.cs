using HWClassLibrary.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    [Serializable]
    internal sealed class ExpressionSyntax : CompileSyntax
    {
        [Node]
        private readonly Defineable _tokenClass;

        [Node]
        internal readonly ICompileSyntax Left;

        [Node]
        private readonly TokenData _token;

        [Node]
        internal readonly ICompileSyntax Right;

        internal ExpressionSyntax(Defineable tokenClass, ICompileSyntax left, TokenData token, ICompileSyntax right)
            : base(token)
        {
            _tokenClass = tokenClass;
            Left = left;
            _token = token;
            Right = right;
        }

        internal override string DumpShort()
        {
            var result = _token.Name;
            if(Left != null)
                result = "(" + Left.DumpShort() + ")" + result;
            if(Right != null)
                result += "(" + Right.DumpShort() + ")";
            return result;
        }

        protected override TokenData GetFirstToken() { return Left == null ? Token : Left.FirstToken; }

        protected override TokenData GetLastToken() { return Right == null ? Token : Right.LastToken; }

        internal override Result Result(ContextBase context, Category category)
        {
            var trace = ObjectId == -51 && category.HasCode;
            StartMethodDumpWithBreak(trace, context, category);
            var result = context.Result(category, Left, _tokenClass, Right);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}