using HWClassLibrary.TreeStructure;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;
using Reni.Syntax;

namespace Reni.Parser
{
    [Serializable]
    sealed internal class ExpressionSyntax : CompileSyntax
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

        internal protected override string DumpShort()
        {
            var result = DefineableToken.Name;
            if(Left != null)
                result = "(" + Left.DumpShort() + ")" + result;
            if(Right != null)
                result += "(" + Right.DumpShort() + ")";
            return result;
        }

        internal protected override Result Result(ContextBase context, Category category)
        {
            var trace = ObjectId == -90 && category.HasRefs;
            StartMethodDumpWithBreak(trace, context, category);
            var result = context.GetResult(category, Left, DefineableToken.TokenClass, Right);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}