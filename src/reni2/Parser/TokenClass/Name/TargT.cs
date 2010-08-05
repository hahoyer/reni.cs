using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    [Serializable]
    internal sealed class TargT : Terminal
    {
        public override Result Result(ContextBase context, Category category, Token token)
        {
            var trace = token.ObjectId == -119 && category.HasRefs;
            StartMethodDump(trace, context, category, token);
            var result = context.CreateArgsReferenceResult(category);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}