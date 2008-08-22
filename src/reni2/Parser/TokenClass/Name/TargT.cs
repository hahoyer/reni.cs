using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    [Token("arg")]
    [Serializable]
    internal sealed class TargT : Terminal
    {
        internal override Result Result(ContextBase context, Category category, Token token)
        {
            var trace = token.ObjectId == 119 && category.HasRefs;
            StartMethodDump(trace, context,category,token);
            var result = context.CreateArgsRefResult(category);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}