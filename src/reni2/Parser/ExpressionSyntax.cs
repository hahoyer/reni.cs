using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.TreeStructure;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser
{
    [Serializable]
    internal sealed class ExpressionSyntax : CompileSyntax
    {
        [Node]
        internal readonly ICompileSyntax Left;

        [Node]
        internal readonly DefineableToken DefineableToken;

        [Node]
        internal readonly ICompileSyntax Right;

        internal ExpressionSyntax(ICompileSyntax left, Token token, ICompileSyntax right)
            : base(token)
        {
            Left = left;
            DefineableToken = new DefineableToken(token);
            Right = right;
        }

        protected internal override string DumpShort()
        {
            var result = DefineableToken.Name;
            if(Left != null)
                result = "(" + Left.DumpShort() + ")" + result;
            if(Right != null)
                result += "(" + Right.DumpShort() + ")";
            return result;
        }

        protected internal override Result Result(ContextBase context, Category category)
        {
            var trace = ObjectId == -57 && category.HasCode;
            StartMethodDumpWithBreak(trace, context, category);
            var result = context.GetResult(category, Left, DefineableToken.TokenClass, Right);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}